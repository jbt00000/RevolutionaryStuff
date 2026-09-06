# RevolutionaryStuff.Azure

Azure SDK integrations including Service Bus messaging, Key Vault, background services, and token credential helpers.

## Service Bus message locks

`ServiceBusBackgroundService` uses the Azure SDK's `ServiceBusProcessor` in `PeekLock` mode. The SDK controls concurrency and automatically renews locks while message handlers run. The service completes successful messages, dead-letters permanent failures, and abandons other processing failures for retry. Graceful shutdown stops receiving and waits for active handlers before disposing the processor so renewal can continue during draining; handlers that never finish can delay shutdown.

Configure the consumer in `ServiceBusWorkerConfig`:

- `MaxMessageLockTime` defaults to `Timeout.InfiniteTimeSpan` (`"-00:00:00.001"` in JSON), allowing automatic renewal for the lifetime of a handler. It controls the total renewal window, not the broker's individual lock duration. Remove any old explicit two-minute value, or set it to infinite, if processing can run longer. Per-execution values override the global setting. A finite value stops renewal after that window even if the handler is still running.
- `MessagePrefetch` defaults to `0`. Leave it at zero for long-running or debugged handlers: prefetched messages are already locked while waiting locally, and their locks are not automatically renewed until processing starts. Remove old nonzero overrides where appropriate.
- `ConcurrentExecutors` defaults to `1` per execution per process. Increase it to process multiple messages concurrently; it is not a cross-process concurrency limit.
- `MessageLockRenewalTimeout` and `RenewalTime` are obsolete and ignored. Renewal timing is managed by the SDK rather than a custom timer.

Set **Lock duration** on the Azure Service Bus **queue or topic subscription** to `00:02:00` for approximately a one-to-two-minute crash-recovery window. This service does not change broker settings or require management permissions. After a crash, another consumer can receive the message after the last renewed lock expires; the remaining delay depends on when renewal last succeeded. Infinite automatic renewal does not create a permanent broker lock.

While a PeekLock remains valid, another consumer cannot receive that same queued message. This is not an absolute concurrency or exactly-once guarantee: network failures, service disruptions, or debugger pauses can prevent renewal and allow redelivery while the original handler is still running. Keep handlers idempotent; strict protection against overlapping side effects requires application-level coordination such as fencing enforced by the target data store. Separate topic subscriptions each receive their own copy and have independent locks.

When Visual Studio pauses all threads, lock renewal pauses too, but Azure's clock keeps running. For debugging, use an isolated queue/subscription and consider its maximum supported lock duration of five minutes; this also increases crash-recovery delay. No in-process renewal loop can keep a lock alive through an arbitrary all-thread pause. After a lock expires, it cannot be renewed or used to settle that delivery; `MessageLockLost` is reported rather than repaired by retrying the same lock.

## What is Revolutionary Stuff
[Revolutionary Stuff](https://revolutionarystuff.com/) is a company [I](https://www.jasonthomas.com/) created a while back to work on creative ideas.

These libraries are a collection of utilities and tools I've created since .NET 1 Beta 2 (don't worry, there have been a few updates since then).

I use these is virtually all of the .NET projects I create to speed my development. 

## License Agreement

Somewhere deep in the GitHub repo and in the NuGet packages you'll find a license agreement.  It's the MIT license.  You can use this code in your projects, commercial or otherwise, with no restrictions.

I offer no warranties of any sort.  But if you find a bug or want to request a change, post a message to the [Issues](https://github.com/jbt00000/RevolutionaryStuff/issues).