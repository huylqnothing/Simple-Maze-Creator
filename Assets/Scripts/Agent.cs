using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField] private float     moveSpeed = 3f;
    private                  Coroutine moveRoutine;

    public void Move(List<GameObject> pathObjects)
    {
        if (pathObjects == null || pathObjects.Count == 0) return;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveAlongPath(pathObjects));
    }

    private IEnumerator MoveAlongPath(List<GameObject> pathObjects)
    {
        foreach (var obj in pathObjects)
        {
            if (obj == null) continue;

            Vector3 targetPos = obj.transform.position;

            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }

            transform.position = targetPos; // fix vị trí chính xác
        }
    }
}