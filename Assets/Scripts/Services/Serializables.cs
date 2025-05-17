using System.Collections.Generic;

[System.Serializable]
public class GameCreationResponse
{
    public string message;
    public string game_id;
}
[System.Serializable]
public class RoleResponse
{
    public string player_id;
    public string name;
    public string facade;
    public string hidden;
    public string detail;
    public bool is_guilty;
}

[System.Serializable]
public class ScenarioResponse
{
    public string title;
    public string intro;
    public string description;
}
[System.Serializable]
public class PlayerRoleResponse
{
    public RoleResponse role;
    public ScenarioResponse scenario;
}
[System.Serializable]
public class PlayerQuestionResponse
{
    public string question;
}
[System.Serializable]
public class SherlockAnswerRequest
{
    public string player_id;
    public string answer;
}
[System.Serializable]
public class SherlockAnswerSummary
{
    public string summary;
}
[System.Serializable]
public class AccusationRequest
{
    public string accuser;
    public string accused;
    public string reason;
}
[System.Serializable]
public class Accusation
{
    public string accuser;
    public string accused;
    public string reason;
}

[System.Serializable]
public class AccusationList
{
    public Accusation[] accusations;
}
[System.Serializable]
public class DefenseRequest
{
    public string player_id;
    public string defense;
}
[System.Serializable]
public class AccusationsResponse
{
    public Dictionary<string, List<string>> accusations;
}
[System.Serializable]
public class PlayerStatus
{
    public string player_id;
    public string name;
    public string facade;
    public string hidden;
    public string detail;
    public bool is_guilty;
}

[System.Serializable]
public class PlayerStrike
{
    public string player_id;
    public int round;
    public string reason;
}
[System.Serializable]
public class StrikesResponses
{
    public List<PlayerStrike> strikes;
}

[System.Serializable]
public class GameStateResponse
{
    public string phase;
    public int round;
    public PlayerStatus[] players;
    public PlayerStrike[] strikes;
}
[System.Serializable]
public class GameWinner
{
    public string message;
    public string player_id;
    public string winner; 
}
[System.Serializable]
public class GameResultResponse
{
    public GameWinner winner;
}