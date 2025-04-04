using UnityEngine;

public class GameManagerComponent : MonoBehaviour {
    GameManager manager;
    private void Awake() {
        manager = new GameManager(new UnityDifficultyJsonLoader("defaultDifficulties"));
    }

    void Start() {
        manager.NewGame();
    }

    private void OnEnable() {
        manager.SubscribeToEvents();
    }

    private void OnDisable() {
        manager.UnSubscribeToEvents();
    }
}
