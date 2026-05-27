using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;
using AgentDashboard.TicketTracking.Infrastructure.Boards;

namespace AgentDashboard.TicketTracking.Infrastructure.UnitTests.Boards;

public sealed class BoardSnapshotCacheShould
{
    private static readonly DateTimeOffset FixedAsOf = new(2026, 05, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void GetLatest_WhenCacheEmpty_ReturnsNull()
    {
        var cache = new BoardSnapshotCache();

        var result = cache.GetLatest();

        result.Should().BeNull();
    }

    [Fact]
    public void LastUpdated_WhenCacheEmpty_ReturnsMinValue()
    {
        var cache = new BoardSnapshotCache();

        var result = cache.LastUpdated;

        result.Should().Be(DateTimeOffset.MinValue);
    }

    [Fact]
    public void GetLatest_AfterUpdate_ReturnsSnapshot()
    {
        var cache = new BoardSnapshotCache();
        var snapshot = BuildTestSnapshot();

        cache.Update(snapshot, FixedAsOf);

        var result = cache.GetLatest();

        result.Should().BeSameAs(snapshot);
    }

    [Fact]
    public void LastUpdated_AfterUpdate_ReturnsAsOfArgument()
    {
        var cache = new BoardSnapshotCache();

        cache.Update(BuildTestSnapshot(), FixedAsOf);

        cache.LastUpdated.Should().Be(FixedAsOf);
    }

    [Fact]
    public void Update_WithNewSnapshot_ReplacesPrevious()
    {
        var cache = new BoardSnapshotCache();
        var firstSnapshot = BuildTestSnapshot(ticketId: 1);
        var secondSnapshot = BuildTestSnapshot(ticketId: 2);

        cache.Update(firstSnapshot, FixedAsOf);
        cache.Update(secondSnapshot, FixedAsOf.AddSeconds(1));

        var result = cache.GetLatest();

        result.Should().BeSameAs(secondSnapshot);
        result.Should().NotBeSameAs(firstSnapshot);
    }

    [Fact]
    public void Update_WithNullSnapshot_ThrowsArgumentNullException()
    {
        var cache = new BoardSnapshotCache();

        var act = () => cache.Update(null!, FixedAsOf);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetLatest_WithConcurrentCalls_ReturnsConsistentState()
    {
        using var cache = new BoardSnapshotCache();
        var firstSnapshot = BuildTestSnapshot(ticketId: 1);
        var secondSnapshot = BuildTestSnapshot(ticketId: 2);
        var exceptions = new List<Exception>();
        var results = new List<BoardSnapshot?>();
        var count = 100;
        var manualReset = new System.Threading.ManualResetEventSlim(false);
        
        // Start writer thread that updates the cache
        var writerThread = new System.Threading.Thread(() =>
        {
            try
            {
                for (int i = 0; i < count; i++)
                {
                    cache.Update(i % 2 == 0 ? firstSnapshot : secondSnapshot, FixedAsOf.AddMilliseconds(i));
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
            finally
            {
                manualReset.Set();
            }
        });
        
        // Start multiple reader threads
        var readerThreads = new System.Threading.Thread[10];
        for (int i = 0; i < readerThreads.Length; i++)
        {
            readerThreads[i] = new System.Threading.Thread(() =>
            {
                try
                {
                    manualReset.Wait();
                    for (int j = 0; j < count; j++)
                    {
                        var result = cache.GetLatest();
                        results.Add(result);
                        var _ = cache.LastUpdated;
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });
        }
        
        writerThread.Start();
        foreach (var readerThread in readerThreads)
        {
            readerThread.Start();
        }
        
        writerThread.Join();
        foreach (var readerThread in readerThreads)
        {
            readerThread.Join();
        }
        
        // Verify no exceptions were thrown
        exceptions.Should().BeEmpty();
        
        // Verify all results are either the first or second snapshot (never null after updates)
        results.Should().AllSatisfy(r => r.Should().NotBeNull());
        results.Should().AllSatisfy(r => 
            r.Should().BeOneOf(firstSnapshot, secondSnapshot));
    }

    // Helper method to create a BoardSnapshot for testing
    private static BoardSnapshot BuildTestSnapshot(int ticketId = 1)
    {
        var column = new BoardColumn(new BoardColumnId("CREATED"), new BoardColumnLabel("Created"));
        var agent = new Agent(new AgentId("DA"), new AgentName("Developer A"), new AgentGlyph("DA"), new AgentRole("developer"));
        var ticket = Ticket.Open(
            new TicketId(ticketId),
            column.Id,
            new TicketTitle("Test Ticket"),
            agent.Id,
            new Retry(0),
            new Age(TimeSpan.Zero),
            thinking: false,
            freshness: TicketFreshness.Neutral);

        return new BoardSnapshot(
            new[] { column },
            new[] { ticket },
            new[] { agent });
    }
}
