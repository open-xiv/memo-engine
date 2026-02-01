using MemoEngine.Engine;
using MemoEngine.Models.Events;


namespace MemoEngine;

public sealed class MemoEngine : System.IDisposable
{
    private readonly EventSink  sink;
    private readonly RuleEngine ruleEngine;

    public Context Context { get; }

    public IGeneralSink   General   => sink.General;
    public IActionSink    Action    => sink.Action;
    public ICombatantSink Combatant => sink.Combatant;
    public IStatusSink    Status    => sink.Status;

    public MemoEngine()
    {
        Context     = new Context();
        ruleEngine  = new RuleEngine(Context);
        sink        = new EventSink(ruleEngine.PostEvent);
    }

    public void Dispose()
    {
        ruleEngine?.Dispose();
    }
}
