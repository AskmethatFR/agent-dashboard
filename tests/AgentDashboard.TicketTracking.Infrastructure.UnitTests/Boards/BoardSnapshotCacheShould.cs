using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;
using AgentDashboard.TicketTracking.Infrastructure.Boards;

namespace AgentDashboard.TicketTracking.Infrastructure.UnitTests.Boards;

// Test List for BoardSnapshotCache:
//   1. GetLatest_WhenCacheEmpty_ReturnsNull
//   2. GetLatest_AfterUpdate_ReturnsSnapshot
//   3. LastUpdated_WhenCacheEmpty_ReturnsMinValue
//   4. LastUpdated_AfterUpdate_ReturnsCurrentTime
//   5. Update_WithNewSnapshot_ReplacesPrevious
//   6. Update_WithNullSnapshot_ThrowsArgumentNullException
//   7. GetLatest_WithConcurrentCalls_ReturnsConsistentState

public sealed class BoardSnapshotCacheShould
{
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
        
        cache.Update(snapshot);
        
        var result = cache.GetLatest();
        
        result.Should().BeSameAs(snapshot);
    }

    [Fact]
    public void LastUpdated_AfterUpdate_ReturnsCurrentTime()
    {
        var cache = new BoardSnapshotCache();
        var beforeUpdate = DateTimeOffset.UtcNow;
        
        cache.Update(BuildTestSnapshot());
        
        var lastUpdated = cache.LastUpdated;
        var afterUpdate = DateTimeOffset.UtcNow;
        
        lastUpdated.Should().BeOnOrAfter(beforeUpdate);
        lastUpdated.Should().BeOnOrBefore(afterUpdate);
    }

    [Fact]
    public void Update_WithNewSnapshot_ReplacesPrevious()
    {
        var cache = new BoardSnapshotCache();
        var firstSnapshot = BuildTestSnapshot(ticketId: 1);
        var secondSnapshot = BuildTestSnapshot(ticketId: 2);
        
        cache.Update(firstSnapshot);
        cache.Update(secondSnapshot);
        
        var result = cache.GetLatest();
        
        result.Should().BeSameAs(secondSnapshot);
        result.Should().NotBeSameAs(firstSnapshot);
    }

    [Fact]
    public void Update_WithNullSnapshot_ThrowsArgumentNullException()
    {
        var cache = new BoardSnapshotCache();
        
        var act = () => cache.Update(null!);
        
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
                    cache.Update(i % 2 == 0 ? firstSnapshot : secondSnapshot);
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
