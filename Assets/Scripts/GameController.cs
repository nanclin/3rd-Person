using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState {
    NewGame,
    Running,
    GameOver,
    Pause,
    NONE
}

public class GameController : MonoBehaviour {

    [SerializeField] private CharController CharController;
    [SerializeField] private UIController UIController;

    private GameState State = GameState.NONE;
    private int Score;
    private List<Collectable> CollectableList = new List<Collectable>();

    private void Start() {
        SetState(GameState.NewGame);

        // register score token objects
        CollectableList.AddRange(FindObjectsOfType<Collectable>());
    }

    private void Update() {
        switch (State) {
            case GameState.NewGame:
            ExecuteStateNewGame();
            break;
            case GameState.Running:
            ExecuteStateRunning();
            break;
            case GameState.GameOver:
            ExecuteStateGameOver();
            break;
            case GameState.Pause:
            ExecuteStatePause();
            break;
        }
    }

    private void SetState(GameState newState) {
        // exit previous state
        Debug.Log("SM - ExitState: " + State.ToString());
        switch (State) {
            case GameState.NewGame:
            ExitStateNewGame();
            break;
            case GameState.Running:
            ExitStateRunning();
            break;
            case GameState.GameOver:
            ExitStateGameOver();
            break;
            case GameState.Pause:
            ExitStatePause();
            break;
        }

        // enter new state
        Debug.Log("SM - EnterState: " + newState.ToString());
        State = newState;
        switch (newState) {
            case GameState.NewGame:
            EnterStateNewGame();
            break;
            case GameState.Running:
            EnterStateRunning();
            break;
            case GameState.GameOver:
            EnterStateGameOver();
            break;
            case GameState.Pause:
            EnterStatePause();
            break;
        }
    }

    // new game state
    private void EnterStateNewGame() {
        Score = 0;
        UIController.SetScore(Score);
        CharController.OnDeath += OnDeath;
        CharController.OnCollectableHit += OnCollectableHit;

        foreach (var item in CollectableList) {
            item.gameObject.SetActive(true);
        }

        SetState(GameState.Running);
    }

    private void ExitStateNewGame() {
        //throw new NotImplementedException();
    }

    private void ExecuteStateNewGame() {
        // TODO: new game intro animation
    }

    // pause state
    private void EnterStatePause() {
        throw new NotImplementedException();
    }

    private void ExitStatePause() {
        throw new NotImplementedException();
    }

    private void ExecuteStatePause() {
        throw new NotImplementedException();
    }

    // running state
    private void EnterStateRunning() {
        //
    }

    private void ExitStateRunning() {
        //throw new NotImplementedException();
    }

    private void ExecuteStateRunning() {
        if (Input.GetKeyDown(KeyCode.Space))
            Score++;
        UIController.SetScore(Score);
    }

    // game over state
    private void EnterStateGameOver() {
        CharController.OnDeath -= OnDeath;
        CharController.OnCollectableHit -= OnCollectableHit;
        SetState(GameState.NewGame);
    }

    private void ExitStateGameOver() {
        //throw new NotImplementedException();
    }

    private void ExecuteStateGameOver() {
        // TODO animate game over screen
    }

    private void OnDeath() {
        SetState(GameState.GameOver);
    }

    private void OnCollectableHit(Collider collider) {
        collider.gameObject.SetActive(false);
        Score++;
        UIController.SetScore(Score);
    }
}
