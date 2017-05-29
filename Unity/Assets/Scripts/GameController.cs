using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class GameController : MonoBehaviour 
{
	public GameObject target;
	public GameObject killZone;
    public GameObject player;
    public Text targetCountText, levelCountText;
    List<GameObject> targetList = new List<GameObject>();
	public int targetsToSpawn = 5;
	public int killzonesToSpawn = 1;
	int score = 0;
    int level = 1;
	public Vector2 stageSize = new Vector2(50, 50);
	
	void Start () 
	{
        StartCoroutine(TargetCheck());
	}

    IEnumerator TargetCheck()
    {
        while(true)
        {
            score = 0;
            ResetPlayer();
            ResetStage();
            if (level % 5 == 0)
                stageSize += new Vector2(25, 25);
            GenerateStage();
            targetCountText.text = "Targets: " + score.ToString() + "/" + targetsToSpawn.ToString();
            levelCountText.text = "Level " + level.ToString();
            yield return new WaitUntil(() => targetList.Count == 0);
            level++;
            targetsToSpawn += 3;
            killzonesToSpawn += 2;
        }
    }

    void ResetStage()
    {
        GameObject[] killzones = GameObject.FindGameObjectsWithTag("killzone");
        foreach (GameObject k in killzones)
            Destroy(k);
        GameObject[] grapples = GameObject.FindGameObjectsWithTag("grapple");
        foreach (GameObject g in grapples)
            Destroy(g);
    }

    void ResetPlayer()
    {
        player.GetComponent<Grapple>().ReleaseGrapple();
        player.transform.position = Vector3.zero;
        player.transform.rotation = Quaternion.identity;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

	void GenerateStage()
	{
		for (int i = 0; i < targetsToSpawn; i++) //Place Targets randomly
		{
			Vector3 spawnPos;
            Collider2D isAreaOccupied = null;
            do {
                isAreaOccupied = null;
				spawnPos = new Vector3 (Random.Range (-stageSize.x, stageSize.x), Random.Range (-stageSize.y, stageSize.y), 0);
                isAreaOccupied = Physics2D.OverlapCircle(spawnPos, 5f);
			} while((Mathf.Abs (spawnPos.x) <= 5 || Mathf.Abs (spawnPos.y) <= 5) || isAreaOccupied != null);
			GameObject tmpTarget = Instantiate (target, spawnPos, Quaternion.identity) as GameObject;
            RegisterTarget(tmpTarget);
		}

		for (int i = 0; i < killzonesToSpawn; i++) //Place killzones randomly
		{
			Vector3 spawnPos;
            Collider2D isAreaOccupied = null;
            do {
                isAreaOccupied = null;
                spawnPos = new Vector3(Random.Range(-stageSize.x, stageSize.x), Random.Range(-stageSize.y, stageSize.y), 0);
                isAreaOccupied = Physics2D.OverlapCircle(spawnPos, 5f);
            } while ((Mathf.Abs(spawnPos.x) <= 5 || Mathf.Abs(spawnPos.y) <= 5) || isAreaOccupied != null);
            Instantiate (killZone, spawnPos, Quaternion.identity);
		}

		//Generate stage bounds
		GameObject l = Instantiate(killZone, new Vector2(-stageSize.x-5, 0), Quaternion.identity) as GameObject;
		l.transform.localScale = new Vector2 (1, stageSize.y);
		GameObject r = Instantiate(killZone, new Vector2(stageSize.x+5, 0), Quaternion.identity) as GameObject;
		r.transform.localScale = new Vector2 (1, stageSize.y);
		GameObject u = Instantiate(killZone, new Vector2(0, stageSize.y+5), Quaternion.identity) as GameObject;
		u.transform.localScale = new Vector2 (stageSize.x, 1);
		GameObject d = Instantiate(killZone, new Vector2(0, -stageSize.y-5), Quaternion.identity) as GameObject;
		d.transform.localScale = new Vector2 (-stageSize.x, 1);
	}

	public void HitTarget(GameObject targetReference)
	{
		score++;
        targetCountText.text = "Targets: " + score.ToString() + "/" + targetsToSpawn.ToString();
        RemoveTargetFromList(targetReference);
	}

	public void ReloadLevel()
	{
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}

    void RegisterTarget(GameObject t)
    {
        targetList.Add(t);
    }

    void RemoveTargetFromList(GameObject t)
    {
        targetList.Remove(t);
    }

}
