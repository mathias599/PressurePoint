using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Tile : MonoBehaviour
{
    public string tileName;
    [SerializeField] protected SpriteRenderer renderer;
    [SerializeField] private GameObject highlight;
    [SerializeField] private bool isWalkable;

    public BaseUnit occupiedUnit;

    public bool walkable => isWalkable && occupiedUnit == null;
    public virtual void Init(int x, int y)
    {

    }

    private void OnMouseEnter()
    {
        highlight.SetActive(true);
        MenuManager.instance.ShowTileInfo(this);
    }

    private void OnMouseExit()
    {
        highlight.SetActive(false);
        MenuManager.instance.ShowTileInfo(null);
    }

    private void OnMouseDown()
    {
        Debug.Log("Tile werd aangeklikt");
        if (GameManager.Instance.State != GameManager.GameState.HeroesTurn)
        {
            Debug.Log("Niet de heroes turn");
            return;
        }

        if (occupiedUnit != null)
        {
            Debug.Log("Tile is niet leeg");
            if (occupiedUnit.faction == Faction.Hero)
            {
                Debug.Log("Hero aangeklikt");
                UnitManager.instance.SetSelectedHero((BaseHero)occupiedUnit);
            }

            else
            {
                Debug.Log("Tegenstander op tile");
                if (UnitManager.instance.selectedHero != null)
                {
                    Debug.Log("Enemy! dood!");
                    var enemy = (BaseEnemy)occupiedUnit;
                    Destroy(enemy.gameObject); //enemy healthbar wordt ook verwijderd door de destroy van de enemy en de knight moet kunnen damage doen
                    UnitManager.instance.SetSelectedHero(null);
                }
            }
        }
        else
        {
            if(UnitManager.instance.selectedHero!= null)
            {
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
                if (hit.collider != null)
                {
                    Tile clickedTile = hit.collider.GetComponent<Tile>();

                    if (clickedTile != null)
                    {
                        if (clickedTile.isWalkable)
                        {
                            SetUnit(UnitManager.instance.selectedHero);
                            UnitManager.instance.SetSelectedHero(null);

                        }
                    }
                }

            }
        }
    }

    public void SetUnit(BaseUnit unit)
    {
        if (unit.occupiedTile != null) unit.occupiedTile.occupiedUnit = null;
        unit.transform.position = transform.position;
        occupiedUnit = unit;
        unit.occupiedTile = this;

    }

}
