using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.Tilemaps;

public class MouseController : MonoBehaviour
{
    [Header("Reference")]
    public GameObject cursor;
    public OverlayTile overlayTile;
    private MapManager mapManager;
    private WeaponBuildManager weaponBuildManager;

    [Header("Building Management")]
    public bool isMounting;
    public bool isRemoving;
    public int towerIndex;

    void Start()
    {
        mapManager = MapManager.Instance;
        weaponBuildManager = GetComponent<WeaponBuildManager>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        RaycastHit2D? hit = GetFocusedOnTile();

        if (hit.HasValue)
        {
            overlayTile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();
            
            if (!overlayTile)
                return;
            
            cursor.transform.position = overlayTile.transform.position;
            cursor.GetComponent<SpriteRenderer>().sortingOrder = overlayTile.GetComponent<SpriteRenderer>().sortingOrder;
            
            #region Mounting
            
            if (isMounting)
            {
                if (overlayTile.turret)
                {
                    Debug.Log("Can't build here");
                    TurretRangeShow(overlayTile, towerIndex, 1, false);
                }
                else
                {
                    TurretRangeShow(overlayTile, towerIndex, 1, true);
                }
            }
            
            if (isMounting && Input.GetMouseButtonDown(0) && !overlayTile.turret)
            {
                weaponBuildManager.MountWeapons(towerIndex, overlayTile);
                isMounting = false;
            }

            #endregion

            #region Hovering
            
            if (overlayTile.turret && !isRemoving && !isMounting)
            {
                TurretRangeShow(overlayTile, overlayTile.turret.type, 1, true);
            }

            #endregion

            #region Removing

            if (overlayTile.turret && isRemoving)
            {
                TurretRangeShow(overlayTile, overlayTile.turret.type, 1, false);
            }

            if (isRemoving && overlayTile.turret && Input.GetMouseButtonDown(0))
            {
                weaponBuildManager.RemoveWeapons(overlayTile);
                isRemoving = false;
            }

            #endregion
        }
    }

    public RaycastHit2D? GetFocusedOnTile()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2d = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2d, Vector2.zero)
                                       .Where(i => i.collider.gameObject.GetComponent<SpriteRenderer>() != null)
                                       .ToArray();

        if(hits.Length > 0)
        {
            return hits.Where(i => i.collider.gameObject.GetComponent<SpriteRenderer>() != null)
                       .OrderByDescending(i => i.collider.transform.position.z).First();
        }

        return null;
    }

    private void TurretRangeShow(OverlayTile overlayTile, int towerIndex, float alpha, bool normal)
    {
        var currentTurret = weaponBuildManager.weaponPrefabs[towerIndex].GetComponent<TurretBehavior>();
        Vector2Int currGrid = new(overlayTile.gridLocation.x, overlayTile.gridLocation.y);
        float rgbValue = normal? 1f : 0.2f;
        foreach (OverlayTile tile in mapManager.GetTilesInRange(currGrid, currentTurret.range))
        {
            tile.GetComponent<SpriteRenderer>().color = new Color(1, rgbValue, rgbValue, alpha);
        }
    }

    public void SetIsMountingAndIndex(int _index)
    {
        isMounting = true;
        isRemoving = false;
        towerIndex = _index;
    }

    public void SetIsRemoving()
    {
        isRemoving = true;
        isMounting = false;
    }
}