using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ZombieController : MonoBehaviour
{
    [Header("Enemy Path Behavior")]
    public OverlayTile currentOverlay;
    public AudioClip[] zombie_groan;
    public AudioClip[] zombie_dying;
    
    [SerializeField]
    private PathFinder _pathFinder;
    private OverlayTile _target;
    private List<OverlayTile> path = new();
    private IsometricCharacterRenderer isoRenderer;
    private Animator _animator;
    private AudioSource source;
    private bool dead;

    [Header("Enemy Attribute")]
    [SerializeField] private float speed = 4.0f;
    [SerializeField] private float normalSpeed; // Serialized for testing now
    [SerializeField] private float reward = 10f;
    [SerializeField] private float health = 2f;
    [SerializeField] private float slowTime;
    private float slowCountDown;

    
    private Vector2Int GetRandomKey(Dictionary<Vector2Int, OverlayTile> dict)
    {
        System.Random random = new System.Random();
        int randomIndex = random.Next(0, dict.Count);
        return new List<Vector2Int>(dict.Keys)[randomIndex];
    }
    
    public void PositionCharacter(OverlayTile tile, bool start)
    {
        if (currentOverlay == tile) return;
        if (currentOverlay != null) 
        { 
            currentOverlay.isBlocked = false;
            currentOverlay.enemyOn = false;
        }
        
        tile.isBlocked = !start;
        var position = tile.transform.position;
        transform.position = new Vector3(position.x, position.y + 0.0001f, position.z);
        GetComponentInChildren<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
        currentOverlay = tile;
        tile.enemyOn = true;
        
        if (start)
        {
            _target = currentOverlay;
        }
    }
    
    void Start()
    {
        _pathFinder = new PathFinder();
        _animator = GetComponentInChildren<Animator>();
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
        source = GetComponent<AudioSource>();
        normalSpeed = speed;
        slowCountDown = 0f;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (dead) return;

        #region Music
        if (Random.value <= 0.0001f && !source.isPlaying)
        {
            int range = Random.Range(0, zombie_groan.Length);
            source.clip = zombie_groan[range];
            source.Play();
        }
        #endregion

        #region Slow Effect

        if (slowCountDown > 0)
        {
            slowCountDown -= Time.deltaTime;
        }
        else
        {
            speed = normalSpeed;
        }

        #endregion
        
        #region Pathing & Moving
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

        #endregion

        #region Being Attacked

        if (currentOverlay.beingShot)
        {
            source.clip = zombie_dying[0];
            source.Play();
            DecreaseHealthAndSpeed(currentOverlay.damageOnThisTile, currentOverlay.shouldSlowed);
        }

        #endregion
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

    private void DecreaseHealthAndSpeed(float damage, bool shouldSlowed)
    {
        health -= damage;
        currentOverlay.damageOnThisTile -= damage;
        currentOverlay.beingShot = false;
        if (shouldSlowed && slowCountDown <= 0)
        {
            speed /= 2;
            slowCountDown = slowTime;
            currentOverlay.shouldSlowed = false;
        }
        StartCoroutine(TurnRed());
        // Checking health status
        if (health <= 0)
        {
            int range = Random.Range(0, zombie_dying.Length);
            source.clip = zombie_dying[range];
            source.Play();
            dead = true;
            _animator.Play("Dying");
        }
    }

    public void KillZombie()
    {
        path.Clear();
        GameManager.Instance.currency += reward;
        currentOverlay.enemyOn = false;
        currentOverlay.isBlocked = false;
        GameManager.Instance.AllZombies.Remove(gameObject);
        Destroy(gameObject);
    }

    private IEnumerator TurnRed()
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        // Check if a SpriteRenderer component is found
        Debug.Log("ture red");
        if (spriteRenderer != null)
        {
            // Get the Material assigned to the SpriteRenderer
            Material spriteMaterial = spriteRenderer.material;
            if (spriteMaterial.HasColor("_Color"))
            {
                spriteMaterial.SetColor("_Color", new Color(1, 0, 0, 1));
                for (float i = 1.0f; i >= 0.0f; i -= 0.1f)
                {
                    spriteMaterial.SetColor("_Color", new Color(1, 0, 0, i));
                    yield return new WaitForSeconds(0.05f);
                }
            }
        }
        else
        {
            Debug.LogError("SpriteRenderer component not found!");
        }
    }
}
