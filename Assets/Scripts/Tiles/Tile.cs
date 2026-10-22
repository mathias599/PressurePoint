using UnityEngine;

public abstract class Tile : MonoBehaviour
{
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
    }

    private void OnMouseExit()
    {
        highlight.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (GameManager.Instance.State != GameManager.GameState.HeroesTurn) return;

        if (occupiedUnit != null)
        {
            if (occupiedUnit.faction == Faction.Hero) UnitManager.instance.SetSelectedHero((BaseHero)occupiedUnit);
            else
            {
                if(UnitManager.instance.selectedHero != null)
                {
                    var enemy = (BaseEnemy)occupiedUnit;
                    Destroy(enemy.gameObject);
                    UnitManager.instance.SetSelectedHero(null);
                }
            }
        }
        else
        {
            if(UnitManager.instance.selectedHero!= null)
            {
                SetUnit(UnitManager.instance.selectedHero);
                UnitManager.instance.SetSelectedHero(null);

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
