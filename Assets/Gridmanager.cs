using UnityEngine;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{
	public int width = 10;
	public int height = 10;
	public float cellSize = 1f;

	public GameObject mouseHighlight;

	private void Update()
	{
		Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

		int x = Mathf.FloorToInt(mouseWorldPosition.x / cellSize);
		int y = Mathf.FloorToInt(mouseWorldPosition.y / cellSize);

		mouseHighlight.transform.position = new Vector3(
			x * cellSize + cellSize / 2,
			y * cellSize + cellSize / 2,
			0
		);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.white;

		for (int x = 0; x <= width; x++)
		{
			Vector3 start = new Vector3(x * cellSize, 0, 0);
			Vector3 end = new Vector3(x * cellSize, height * cellSize, 0);

			Gizmos.DrawLine(start, end);
		}

		for (int y = 0; y <= height; y++)
		{
			Vector3 start = new Vector3(0, y * cellSize, 0);
			Vector3 end = new Vector3(width * cellSize, y * cellSize, 0);

			Gizmos.DrawLine(start, end);
		}
	}

}