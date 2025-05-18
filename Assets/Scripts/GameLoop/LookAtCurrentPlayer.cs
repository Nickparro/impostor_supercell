using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class LookAtCurrentPlayer : NetworkBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private bool smoothRotation = true;

    [SerializeField] private GameObject highlightEffect;

    private Transform currentTargetTransform;
    private Coroutine lookRoutine;

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        if (highlightEffect != null)
        {
            highlightEffect.SetActive(false);
        }
    }

    private void Update()
    {
        if (!IsServer && !IsClient) return;

        if (gameManager.CurrentPhase.Value == GamePhase.Questions)
        {
            if (gameManager.currentPlayerIndex.Value >= 0)
            {
                var players = PlayerData.AllPlayers.Values;
                int currentIndex = 0;

                foreach (var player in players)
                {
                    if (!player.IsEliminated.Value && currentIndex == gameManager.currentPlayerIndex.Value)
                    {
                        if (player.transform != null && currentTargetTransform != player.transform)
                        {
                            currentTargetTransform = player.transform;
                            if (smoothRotation)
                            {
                                if (lookRoutine != null)
                                {
                                    StopCoroutine(lookRoutine);
                                }
                                lookRoutine = StartCoroutine(SmoothLookAtPlayer());
                            }
                            if (highlightEffect != null)
                            {
                                highlightEffect.SetActive(true);
                            }
                        }
                        break;
                    }

                    if (!player.IsEliminated.Value)
                    {
                        currentIndex++;
                    }
                }

                if (!smoothRotation && currentTargetTransform != null)
                {
                    LookAtTargetYOnly();
                }
            }
            else
            {
                currentTargetTransform = null;
                if (highlightEffect != null)
                {
                    highlightEffect.SetActive(false);
                }
            }
        }
        else
        {
            currentTargetTransform = null;
            if (highlightEffect != null)
            {
                highlightEffect.SetActive(false);
            }
        }
    }

    private IEnumerator SmoothLookAtPlayer()
    {
        while (currentTargetTransform != null)
        {
            LookAtTargetYOnly();
            yield return null;
        }
    }

    private void LookAtTargetYOnly()
    {
        if (currentTargetTransform == null) return;

        Vector3 direction = currentTargetTransform.position - transform.position;
        direction.y = 0; 

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            if (smoothRotation)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }
    }
}
