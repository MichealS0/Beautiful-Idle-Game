using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieController : MonoBehaviour
{
    public OverlayTile currentOverlay;
    [SerializeField]
    private float speed = 4.0f;
    private PathFinder _pathFinder;
    private OverlayTile _target;
    private List<OverlayTile> path = new();
    private IsometricCharacterRenderer isoRenderer;
    
    private Vector2Int GetRandomKey(Dictionary<Vector2Int, GameObject> dict)
    {
        System.Random random = new System.Random();
        int randomIndex = random.Next(0, dict.Count);
        return new List<Vector2Int>(dict.Keys)[randomIndex];
    }
    
    public void PositionCharacter(OverlayTile tile, bool start)
    {
        if (currentOverlay == tile) return;
        if (currentOverlay != null) { currentOverlay.isBlocked = false; }
        
        tile.isBlocked = !start;
        var position = tile.transform.position;
        transform.position = new Vector3(position.x, position.y + 0.0001f, position.z);
        GetComponentInChildren<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
        currentOverlay = tile;
        
        if (start)
        {
            _target = currentOverlay;
        }
    }
    
    void Start()
    {
        _pathFinder = new PathFinder();
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
    }
    
    // Update is called once per frame
    void Update()
    {
        // every
        if (path.Count == 0)
        {
            // pick a random location on the map
            var randKey = GetRandomKey(MapManager.Instance.map);
            var newTarget = MapManager.Instance.map[randKey].GetComponent<OverlayTile>();
            _target = newTarget;
            path = _pathFinder.FindPath(currentOverlay, _target, 1);
        }
        else if (path.Count > 0 && path[0].isBlocked)
        {
            path = _pathFinder.FindPath(currentOverlay, _target, 1);
        }

        if (path.Count > 0)
        {
            MoveAlongPath();
        }
    }

    private void MoveAlongPath()
    {
        var step = speed * Time.deltaTime;
        var zIndex = path[0].transform.position.z;
        var previousPosition = transform.position;
        transform.position = Vector2.MoveTowards(transform.position, path[0].transform.position, step);
        transform.position = new Vector3(transform.position.x, transform.position.y, zIndex);
        
        isoRenderer.SetDirection(  transform.position - previousPosition);

        if (Vector2.Distance(transform.position, path[0].transform.position) < 0.001f)
        {
            PositionCharacter(path[0], false);
            path.RemoveAt(0);
        }
    }
    
}