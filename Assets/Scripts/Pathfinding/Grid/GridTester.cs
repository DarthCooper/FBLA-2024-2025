using UnityEngine;
using UnityEngine.InputSystem;
using CodeMonkey.Utils;
using UnityEditor;

[ExecuteInEditMode]
public class GridTester : MonoBehaviour
{
    private Grid grid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grid = new Grid(4, 2, 5f, new Vector3(0, 0));
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 MousePos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(MousePos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.DrawRay(MousePos, Camera.main.ScreenToWorldPoint(MousePos) - hit.point);
                Vector3 pointConversion = new Vector3(hit.point.x, hit.point.z, 0);
                grid.SetValue(pointConversion, 56);
            }
        }

        if(Input.GetMouseButtonDown(1))
        {
            Vector2 MousePos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(MousePos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 pointConversion = new Vector3(hit.point.x, hit.point.z, 0);
                Debug.Log(grid.GetValue(pointConversion));
            }
        }
    }
}
