﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MonsterBar : MonoBehaviour {

//	public GameObject protoMonster;
//	private float mouseOffsetX;
//	private float dragOffsetX;
//	private float dragX;
//	private float destDragX;
//	private bool dragging = false;
//	private Vector3 startPos;
//	bool isMoveTo = false;
	public static MonsterBar Instance;



	public delegate void onMonsterChangeDelegate(Monster monster);
	public onMonsterChangeDelegate onMonsterLocated;
	public onMonsterChangeDelegate onMonsterDrag;


	public int NumMonsters;

	public Button ButtonPrevious;
	public Button ButtonNext;

	public Slider MonsterGageSlider;
	public Image Icon_gage_1;
	public Image Icon_gage_2;
	public Image Icon_gage_3;





	float spacing;
	int currentIndex = 0;
	Transform Content;
	Transform selectedMonster = null;

	float toX = 0;
	bool isDrag = false;
	bool isLocated = true;
	bool isTouchAllow = false;



	public int CurrentMonsterIndex
	{
		get {
			return currentIndex;
		}

		set{ 
			currentIndex = value;
			Invoke("jumpToNewPosition", 0.001f);
		}
	}

	void jumpToNewPosition()
	{
		Transform t = Content.GetChild (currentIndex);
		toX = Content.transform.position.x - t.position.x;

		Vector2 newPos = new Vector2 (toX, Content.position.y);
		Content.position = newPos;

		isDrag = false;
		updateButtons ();
	}


	void Awake()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start () {
		Content = transform.GetChild (0);
//		startPos = Content.position;
	}


	// Update is called once per frame
	void Update () {

		if (selectedMonster != null) {
			if (isLocated == false) {
				selectedMonster.localScale = Vector3.Lerp (selectedMonster.localScale, transform.localScale, Time.deltaTime * 5);
//				selectedMonster.position = Vector3.Lerp (selectedMonster.position, transform.position, Time.deltaTime * 5);
				if (!IsMiniGame) {
					selectedMonster.position = Vector3.Lerp (selectedMonster.position, selectedMonster.GetComponent<MonsterPosition> ().GameScreen, Time.deltaTime * 5);
				}

				float dis1 = Vector2.Distance (selectedMonster.localScale, transform.localScale);
				float dis2 = Vector2.Distance (selectedMonster.position, selectedMonster.GetComponent<MonsterPosition> ().GameScreen);

				if (dis1 < 0.001f && dis2 < 0.001f) {
					isLocated = true;
					selectedMonster = null;
				}
			}
			return;
		} else if (Content.childCount == 0) {
			return;
		}

		if (isDrag == false && Content.transform.position.x != toX) {
			Vector2 newPos = new Vector2 (toX, Content.position.y);
			Content.position = Vector3.MoveTowards (Content.position, newPos, Time.deltaTime * 20);

			if (Vector2.Distance (Content.position, newPos) < 0.001f) {
				if (onMonsterLocated != null) {
					Monster m = Content.GetChild (currentIndex).GetComponent<MonsterSpritesController> ().monster;
					onMonsterLocated (m);
				}
			}
		}
	}


	bool IsMiniGame = false;


	public void Init(bool isMiniGame = false)
	{
		IsMiniGame = isMiniGame;
		selectedMonster = null;
		isLocated = true;
		isTouchAllow = true;
//		currentIndex = 0;

		Destroy (GameObject.Find ("monster"));

		Clean ();

		hideGageIcons ();

		//spacing = Screen.width * 1.45f;
		spacing = Screen.width * 2.0f;
		spacing = Camera.main.ScreenToWorldPoint(new Vector3(spacing,0,0)).x;

		var friends = UserInfo.Instance.CollectedFriends;

		int numReady = 0;

		for (int monsterIndex = 0; monsterIndex < friends.Length; monsterIndex++) {
			Monster monster = friends [monsterIndex];
			if (monster != null) {
				//GameObject newMonster = Instantiate (protoMonster, new Vector3 (), Quaternion.identity) as GameObject;
				GameObject newMonster = Instantiate (monster.currentPlayGO, new Vector3 (), Quaternion.identity) as GameObject;

				newMonster.transform.parent = Content;

				if (IsMiniGame) {
					newMonster.GetComponent<MonsterPosition> ().setMiniGamePosition ();
				} else {
					newMonster.GetComponent<MonsterPosition> ().setGamePosition ();
				}

				newMonster.transform.localPosition += new Vector3 (spacing * numReady, 0, 0);

				newMonster.transform.localScale = new Vector3 (.8f, .8f, .8f);
				newMonster.AddComponent<MonsterSpritesController> ().monster = monster;

				updateMonsterEmotion (IsMiniGame, newMonster, monster);

				numReady++;
			}
		}
		NumMonsters = numReady;

		if (onMonsterLocated != null) {
			Monster m = Content.GetChild (currentIndex).GetComponent<MonsterSpritesController> ().monster;
			onMonsterLocated (m);
		}

		Invoke("jumpToNewPosition", 0.01f);
//		Invoke("updateButtons", 0.011f);
	}

	void updateMonsterEmotion (bool isMiniGame, GameObject monsterGO, Monster monster)
	{
		Animator animController = null;
		if (monsterGO) {
			var obj = monsterGO.GetComponentInChildren<MonsterAnimation> ();
			if (obj != null) {
				animController = obj.gameObject.GetComponent<Animator> ();
			}

			if (animController != null) {
				if (isMiniGame == false || monster.EmotionType == MonsterEmotionTypes.Happy) {
					animController.SetBool ("IsSad", false);
				} else {
					animController.SetBool ("IsSad", true);
				}
			}
		}
	}


	public void Select() {
		selectedMonster = Content.GetChild (currentIndex);
		selectedMonster.transform.parent = null;
		for (int i = 0; i < Content.childCount; i++) {
			Destroy (Content.GetChild (i).gameObject);
		}
		selectedMonster.gameObject.name = "monster";
		isLocated = false;
		isTouchAllow = false;
		//selectedMonster.gameObject.tag = "CurrentMonster";
	}


	public void Clean() {
		for (int i = 0; i < Content.childCount; i++) {
			Destroy (Content.GetChild (i).gameObject);
		}
	}

	public void MoveToPrevious()
	{
		if(currentIndex - 1 >= 0)
		{
			if (onMonsterDrag != null) {
				onMonsterDrag (null);
			}

			currentIndex--;

			setNewIndexPosition ();
		}
	}

	public void MoveToNext()
	{
		if(currentIndex + 1 < Content.childCount)
		{
			if (onMonsterDrag != null) {
				onMonsterDrag (null);
			}
			currentIndex++;
			setNewIndexPosition ();
		}
	}

	void setNewIndexPosition()
	{
		Transform t = Content.GetChild (currentIndex);
		toX = Content.transform.position.x - t.position.x;
		isDrag = false;
		updateButtons ();
	}

	public void updateButtons()
	{
		if (ButtonNext != null) {
			if (currentIndex < (Content.childCount - 1)) {
				ButtonNext.interactable = true;
			} else {
				ButtonNext.interactable = false;
			}

/*
			if (currentIndex == 0) {
				ButtonNext.interactable  = true;	
			} else {
				ButtonNext.interactable  = false;	
			}
*/
		}

		if (ButtonPrevious != null) {

			if (currentIndex > 0) {
				ButtonPrevious.interactable  = true;
			} else {
				ButtonPrevious.interactable  = false;
			}
/*
			if (currentIndex == Content.childCount - 1) {
				ButtonPrevious.interactable  = true;
			} else {
				ButtonPrevious.interactable  = false;
			}
*/
		}
		updateMonsterSlider ();
	}

	void updateMonsterSlider()
	{
		if (MonsterGageSlider != null) {
			Monster monster = Content.GetChild (currentIndex).GetComponent<MonsterSpritesController> ().monster;

			if (monster != null) {
		
				if (monster.IsReady) {
					MonsterGageSlider.gameObject.SetActive (false);
				} else {
					MonsterGageSlider.gameObject.SetActive (true);
					MonsterGageSlider.value = monster.GageValue;
					UpdateGageSprite (monster);
				}

			} else {
				MonsterGageSlider.gameObject.SetActive (false);
			}
		}
	}


	public void DragBy(float moveX)
	{
		Vector2 newPos = new Vector2 (Content.transform.position.x + moveX, Content.transform.position.y);

		Content.transform.position = newPos;
	}

	public void OnBeginDrag()
	{
		if (isTouchAllow) {
			isDrag = true;
			if (onMonsterDrag != null) {
				onMonsterDrag (null);
			}
		}

	}

	public void OnEndDrag()
	{
		if (isTouchAllow) {

			isDrag = false;

			float minDistance = float.MaxValue;
			int minIndex = 0;
			for (int i = 0; i < Content.childCount; i++) {
				float dist = Vector3.Distance (Content.GetChild (i).transform.position, transform.position);

				if (i == currentIndex) {
					dist = dist * 1.5f;
				}

//				if (i != currentIndex && (minDistance == float.MaxValue || dist < minDistance)) {
				if (minDistance == float.MaxValue || dist < minDistance) {
					minDistance = dist;
					minIndex = i;
				}
			}

			float currDist = Vector3.Distance (Content.GetChild (currentIndex).transform.position, transform.position);

			if (minDistance < currDist * 4.0f) {

				Transform t = Content.GetChild (minIndex);
				toX = Content.transform.position.x - t.position.x;

				currentIndex = minIndex;

				updateButtons ();
			}
		}
	}

	public void addBackMonster(GameObject newMonster)
	{
		newMonster.transform.parent = Content;
		newMonster.GetComponent<MonsterPosition> ().setGamePosition ();

//		newMonster.transform.localPosition += new Vector3 (spacing * numReady, 0, 0);
//		newMonster.transform.localScale = new Vector3 (.8f, .8f, .8f);
	}

	void hideGageIcons ()
	{
		if (Icon_gage_1) {
			Icon_gage_1.gameObject.SetActive (false);
		}
		if (Icon_gage_2) {
			Icon_gage_2.gameObject.SetActive (false);
		}
		if (Icon_gage_3) {
			Icon_gage_3.gameObject.SetActive (false);
		}
	}

	void UpdateGageSprite(Monster monster)
	{
		hideGageIcons ();
		if (monster.Gage == 0) {
			Icon_gage_1.gameObject.SetActive (true);
		} else if (monster.Gage == 1) {
			Icon_gage_2.gameObject.SetActive (true);
		} else if (monster.Gage == 2) {
			Icon_gage_3.gameObject.SetActive (true);
		}

	}


}
