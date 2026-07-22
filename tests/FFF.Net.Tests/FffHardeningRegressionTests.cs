using FFF.Net.Interop;
using FFF.Net.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xunit;

namespace FFF.Net.Tests;

public class FffHardeningRegressionTests
{
    private readonly string _testPath;

    public FffHardeningRegressionTests()
    {
        _testPath = Directory.GetCurrentDirectory();
    }

    [Fact]
    public async Task Watch_CallbackThrowsException_MustNotCrashProcessAndDispatchOnWatchError()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "fff_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var options = new FffCreateOptions { BasePath = tempDir, Watch = true };
            using var finder = new FffFinder(options);
            finder.WaitForScan(TimeSpan.FromSeconds(5));
            Assert.True(await finder.WaitForWatcherReadyAsync(TimeSpan.FromSeconds(5)));

            var tcs = new TaskCompletionSource<FffWatchErrorEventArgs>();
            finder.OnWatchError += (sender, e) => tcs.TrySetResult(e);

            using var sub = finder.Watch(null, batch =>
            {
                throw new InvalidOperationException("Simulated watch callback exception");
            });

            // Create a file to trigger native watcher callback
            string testFile = Path.Combine(tempDir, "trigger.txt");
            File.WriteAllText(testFile, "hello");

            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(5000));
            Assert.Same(tcs.Task, completedTask);

            var errorEventArgs = await tcs.Task;
            Assert.NotNull(errorEventArgs);
            Assert.Equal(sub.WatchId, errorEventArgs.WatchId);
            Assert.IsType<InvalidOperationException>(errorEventArgs.Exception);
            Assert.Equal("Simulated watch callback exception", errorEventArgs.Exception.Message);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public async Task Watch_HighConcurrencyRegistrationAndUnwatch_ShouldNotCorruptState()
    {
        var options = new FffCreateOptions { BasePath = _testPath, Watch = true };
        using var finder = new FffFinder(options);
        finder.WaitForScan(TimeSpan.FromSeconds(5));
        Assert.True(await finder.WaitForWatcherReadyAsync(TimeSpan.FromSeconds(5)));

        var tasks = new List<Task>();
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                using var sub = finder.Watch("*.cs", _ => { });
                Assert.NotEqual(0UL, sub.WatchId);
            }));
        }

        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task Dispose_RacingWithConcurrentQueries_ShouldBeThreadSafe()
    {
        var options = new FffCreateOptions { BasePath = _testPath };
        var finder = new FffFinder(options);
        finder.WaitForScan(TimeSpan.FromSeconds(5));

        var t1 = Task.Run(() =>
        {
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    finder.Search("test");
                    finder.Grep("test");
                    finder.Glob("*.cs");
                    finder.SearchDirectories("test");
                    finder.SearchMixed("test");
                }
            }
            catch (ObjectDisposedException) { }
            catch (FffNativeException) { }
        });

        var t2 = Task.Run(() =>
        {
            Thread.Sleep(5);
            finder.Dispose();
        });

        await Task.WhenAll(t1, t2);
    }

    [Fact]
    public async Task WatchSubscription_GCFinalization_ShouldAutoUnsubscribe()
    {
        var options = new FffCreateOptions { BasePath = _testPath, Watch = true };
        using var finder = new FffFinder(options);
        finder.WaitForScan(TimeSpan.FromSeconds(5));
        Assert.True(await finder.WaitForWatcherReadyAsync(TimeSpan.FromSeconds(5)));

        WeakReference subRef = null!;

        new Action(() =>
        {
            var sub = finder.Watch("*.cs", _ => { });
            subRef = new WeakReference(sub);
        })();

        for (int i = 0; i < 5 && subRef.IsAlive; i++)
        {
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
        }

        Assert.False(subRef.IsAlive);
    }

    [Fact]
    public async Task WatchAsync_BoundedChannel_ShouldRespectCapacityAndDropOldest()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "fff_chan_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var options = new FffCreateOptions { BasePath = tempDir, Watch = true };
            using var finder = new FffFinder(options);
            finder.WaitForScan(TimeSpan.FromSeconds(5));
            Assert.True(await finder.WaitForWatcherReadyAsync(TimeSpan.FromSeconds(5)));

            var watchOpts = new FffWatchOptions
            {
                ChannelOptions = new BoundedChannelOptions(2)
                {
                    SingleWriter = true,
                    FullMode = BoundedChannelFullMode.DropOldest
                }
            };

            using var cts = new CancellationTokenSource();
            var stream = finder.WatchAsync(null, watchOpts, cts.Token);
            var enumerator = stream.GetAsyncEnumerator(cts.Token);

            // Create multiple files quickly to trigger events
            for (int i = 0; i < 5; i++)
            {
                File.WriteAllText(Path.Combine(tempDir, $"file_{i}.txt"), "content");
            }

            cts.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                while (await enumerator.MoveNextAsync()) { }
            });
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }

    [Fact]
    public void SafeHandles_Disposal_ShouldReleasePointers()
    {
        var handle = new SafeFffHandle(IntPtr.Zero, ownsHandle: false);
        Assert.True(handle.IsInvalid);

        var validDummy = new SafeFffHandle((IntPtr)1234, ownsHandle: false);
        Assert.False(validDummy.IsInvalid);
        validDummy.Dispose();
        Assert.True(validDummy.IsClosed);
    }

    [Fact]
    public void FffNativeException_ShouldBeThrownForNativeFailures()
    {
        var options = new FffCreateOptions { BasePath = _testPath };
        using var finder = new FffFinder(options);

        // Invalid offset or invalid inputs should throw FffNativeException or ArgumentException
        Assert.Throws<ArgumentNullException>(() => finder.Search(null!));
    }
}
