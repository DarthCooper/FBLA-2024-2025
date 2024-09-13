using UnityEngine;
using CodeMonkey.Utils;

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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log(hit.point);
                Vector3 pointConversion = new Vector3(hit.point.x, hit.point.z, 0);
                grid.SetValue(pointConversion, 56);
            }
        }

        if(Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 pointConversion = new Vector3(hit.point.x, hit.point.z, 0);
                Debug.Log(grid.GetValue(pointConversion));
            }
        }
    }
}
