using Godot;

namespace BlueLine.Management;

public partial class Scoreboard : Node
{
    #region Members

    private int _homeScore = 0;
    private int _awayScore = 0;
    private double _gameTime;
    private bool _isPaused;
    private Label _homeScoreLbl;
    private Label _awayScoreLbl;
    private Label _gameTimeLbl;
    private bool _hasPeriodStarted;
    private GameState _prevState;

    #endregion Members

    #region Properties

    /// <summary>
    /// How long each period lasts in seconds.
    /// </summary>
    [Export]
    public double GameClockInSeconds = 1200;

    #endregion Properties

    #region Overrides

    public override void _Ready()
    {
        GameEvents.Instance.PuckDropped += On_PuckDropped;
        GameEvents.Instance.GoalScored += On_GoalScored;
        GameEvents.Instance.ChangeGameState += On_GameState_Changed;

        _gameTimeLbl = GetNode<Label>("Time");
        _homeScoreLbl = GetNode<Label>("Home Score");
        _awayScoreLbl = GetNode<Label>("Away Score");
    }

    public override void _Process(double delta)
    {
        if (_isPaused || _gameTime <= 0) return;

        _gameTime -= delta;
        
        if (_gameTime < 0)
        {
            _gameTime = 0;
            On_TimerFinished();
        }

        UpdateTimerLabel();
    }

    #endregion Overrides

    #region Public Methods

    /// <summary>
    /// Pauses the game time.
    /// </summary>
    public void PauseTimer()
    {
        _isPaused = true;
    }

    /// <summary>
    /// Resumes the game time.
    /// </summary>
    public void ResumeTimer()
    {
        _isPaused = false;
    }

    /// <summary>
    /// Resets the game time to the given period length.
    /// </summary>
    public void ResetTimer()
    {
        _gameTime = GameClockInSeconds;
        _isPaused = false;
        UpdateTimerLabel();
    }

    #endregion Public Methods

    #region Private Methods

    private void UpdateTimerLabel()
    {
        if (_gameTimeLbl == null) return;

        int mins = (int)(_gameTime / 60);
        int secs = (int)(_gameTime % 60);

        _gameTimeLbl.Text = $"{mins:D2}:{secs:D2}";
    }

    #endregion Private Methods

    #region Events

    private void On_GoalScored(bool homeGoal)
    {
        PauseTimer();

        if (homeGoal)
        {
            _homeScore += 1;
            _homeScoreLbl.Text = $"{_homeScore:00}";
        }
        else
        {
            _awayScore += 1;
            _awayScoreLbl.Text = $"{_awayScore:00}";
        }
    }

    private void On_PuckDropped(FaceoffDot _)
    {
        if (_hasPeriodStarted)
        {
            ResumeTimer();
        }
        else
        {
            _hasPeriodStarted = true;
            ResetTimer();
        }
    }

    private void On_TimerFinished()
    {
        // TODO: signal end of period.
        MatchStatus.Instance.State = GameState.EndOfPeriod;
        _hasPeriodStarted = false;
    }

    private void On_GameState_Changed(GameState state)
    {
        if (state == GameState.Paused)
        {
            PauseTimer();
        }
        else if (state == GameState.Playing &&
                _prevState == GameState.Paused)
        {
            ResumeTimer();
        }

        _prevState = state;
    }

    #endregion Events
}