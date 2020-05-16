﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public AgentManager parent1;
    public AgentManager parent2;

    public float Ph;
    public float Pp;

    public int vitality;
    public int endurance;
    public int strength;
    public int gluttony; // Обжорство

    public int health;
    public int hungry;
    public int leisure;

    public bool isLeisure;
    public bool isLeisureGo;

    private static int MAX = 50;
    private static int hungryMAX;

    public int hunger;

    UnitTasks ut;
    BehaviourManager bm;
    FieldOfView fov_Look;
    FieldOfView fov_Near;

    // List of knowable agents
    Dictionary<GameObject, SympathyManager> sym_agents;

    // public AgentManager(float Ph, float Pp, int vitality, int endurance, int strength, int gluttony, AgentManager parent1, AgentManager parent2)
    // {
    //     this.Ph = Ph;
    //     this.Pp = Pp;
    //     this.vitality = vitality;
    //     this.endurance = endurance;
    //     this.strength = strength;
    //     this.gluttony = gluttony;

    //     this.parent1 = parent1;
    //     this.parent2 = parent2;
    // }

    public void NewAM(float Ph, float Pp, int vitality, int endurance, int strength, int gluttony, AgentManager parent1, AgentManager parent2)
    {
        this.Ph = Ph;
        this.Pp = Pp;

        this.vitality = vitality;
        this.endurance = endurance;
        this.strength = strength;
        this.gluttony = gluttony;

        this.parent1 = parent1;
        this.parent2 = parent2;

        Unit unit = GetComponent<Unit>();
        unit.target = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Target")).transform;

        InteractableManager im = GetComponent<InteractableManager>();
        var foods = FindObjectsOfType<FoodBehaviour>();
        List<GameObject> foodList = new List<GameObject>();
        foreach (var food in foods)
        {
            foodList.Add(food.gameObject);
        }
        im.foodDisp = foodList;

        var plays = FindObjectsOfType<PlayBehaviour>();
        List<GameObject> playList = new List<GameObject>();
        foreach (var play in plays)
        {
            playList.Add(play.gameObject);
        }
        im.plesureDisp = playList;
    }

    // Start is called before the first frame update
    void Start()
    {
        health = (int)(MAX * (1f + vitality / 100f));
        leisure = MAX;
        hungryMAX = (int)(MAX * (1f + gluttony / 100f));

        ut = GetComponent<UnitTasks>();
        bm = FindObjectOfType<BehaviourManager>();

        // Hungry
        isStartHungry = true;

        timer = 0;
        time_day = bm.time;
        hungry = hungryMAX;
        hunger = (int)(hungryMAX * Ph);

        isDecision = true;

        _Ph = 1;
        _Pp = 1;

        // Field of View
        var fovs = GetComponents<FieldOfView>();

        foreach (var fov in fovs)
        {
            if (fov.isNearTarget)
                fov_Near = fov;
            else
                fov_Look = fov;
        }

        sym_agents = new Dictionary<GameObject, SympathyManager>();
    }

    private bool isHungry;
    private void LateUpdate()
    {
        if (hungry <= hungryMAX * 0.8 && hungry > hungryMAX * 0.6)
        {
            if (!isHungry)
            {
                isHungry = true;
                StopAllCoroutines();
                StartCoroutine(Hunger(1));
            }
        }
        else if (hungry <= hungryMAX * 0.6 && hungry > hungryMAX * 0.4)
        {
            if (!isHungry)
            {
                isHungry = true;
                StopAllCoroutines();
                StartCoroutine(Hunger(3));
            }
        }
        else if (hungry <= hungryMAX * 0.4)
        {
            if (!isHungry)
            {
                isHungry = true;
                StopAllCoroutines();
                StartCoroutine(Hunger(5));
            }
        }
    }

    IEnumerator Hunger(int value)
    {
        health -= value;
        yield return new WaitForSeconds(3);
        isHungry = false;
    }

    private bool isStartHungry;
    [HideInInspector]
    public float timer;
    private float time_day;

    [HideInInspector]
    public bool isDecision;
    private float _Ph;
    private float _Pp;
    // Update is called once per frame
    void Update()
    {
        if (isStartHungry)
        {
            timer += Time.deltaTime;

            if (timer >= time_day / 4)
            {
                timer = 0;
                hungry -= 5;
            }
        }

        if (isDecision)
        {
            _Ph *= Ph;
            _Pp *= Pp;

            if (_Ph >= _Pp)
            {
                if (hungry <= hunger)
                {
                    if (bm.highnoon)
                        Eat();
                }

                Walk();
                _Pp = 1;
            }
            else
            {
                if (hungry <= hunger)
                {
                    if (bm.highnoon)
                        Eat();
                }
                var choice = Random.Range(0f, 1f);
                if (choice > _Pp)
                {
                    Walk();
                }
                else
                {
                    Play();
                }

                _Ph = 1;
            }
        }

        if(fov_Look.visibleTargets.Count > 0)
        {
            var fov_List = fov_Look.visibleTargets;
            foreach(var agent in fov_List)
            {
                if(sym_agents.ContainsKey(agent))
                {
                    // Debug.Log("Yes!");
                    //Do something
                } else 
                {
                    sym_agents.Add(agent, new SympathyManager(agent, 0));
                    // Debug.Log("Added");
                }
            }
        }

        if(fov_Near.visibleTargets.Count > 0)
        {
            var fov_List = fov_Near.visibleTargets;
            foreach(var agent in fov_List)
            {
                if(sym_agents.ContainsKey(agent))
                {
                    if(!isNear)
                    {
                        AddedSympathy(agent, 5);
                        isNear = true;
                        Debug.Log(sym_agents[agent].ToString());

                        if(sym_agents[agent].isHaveABaby)
                        {

                        }
                    }
                }
            }
        } else 
        {
            isNear = false;
        }

        if (health <= 0)
        {
            Died();
        }
    }

    bool isNear;
    GameObject lover;
    delegate bool Love(GameObject l);
    void AddedSympathy(GameObject agent, int count)
    {
        Love love = l => l != null;
        sym_agents[agent].PlusSym(count, love(lover), this);
    }
    void Walk()
    {
        if (!ut.isHungry)
        {
            ut.isWalking = true;
            isDecision = false;
        }

    }

    void Play()
    {
        ut.isPlaying = true;
        isDecision = false;
    }

    void Eat()
    {
        if (ut.isWalking)
            ut.DoneWalk();
        if (ut.isPlaying)
            ut.DonePlay();

        Debug.Log("EATING");
        ut.isHungry = true;
        isDecision = false;
    }

    void Died()
    {
        this.gameObject.SetActive(false);
    }
}
