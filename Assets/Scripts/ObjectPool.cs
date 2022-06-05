using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectPool : MonoBehaviour
{
    // 싱글턴 내부에서만 접근 가능
    static ObjectPool inst;
    void Awake() => inst = this;

    [Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    [SerializeField] Pool[] pools;
    List<GameObject> spawnObjects;  //모든 게임오브젝트를 담는곳
    Dictionary<string, Queue<GameObject>> poolDictionary;    //태그에 맞는 오브젝트

    public static GameObject SpawnFromPool(string tag, Vector3 position) =>
        inst._SpawnFromPool(tag, position, Quaternion.identity);

    // 위치까지만 적으면 Quaternion값이 알아서 들어감(메소드 오버로딩)
    public static GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation) =>
        inst._SpawnFromPool(tag, position, rotation);

    public static T SpawnFromPool<T>(string tag, Vector3 position) where T : Component
    {
        //하나의 게임 오브젝트를 리턴받음
        GameObject obj = inst._SpawnFromPool(tag, position, Quaternion.identity);

        //컴포넌트를 꺼내면서 if문 비교 그리고 컴포넌트가 존재하면 리턴
        if (obj.TryGetComponent(out T Component))
            return Component;
        else
        {
            //존재하지 않으면
            obj.SetActive(false);
            throw new Exception($"Componet not found");
        }
    }
    //회전 값이 있으면
    public static T SpawnFromPool<T>(string tag, Vector3 position, Quaternion rotation) where T : Component
    {
        //하나의 게임 오브젝트를 리턴받음
        GameObject obj = inst._SpawnFromPool(tag, position, rotation);

        //컴포넌트가 존재하면 리턴
        if (obj.TryGetComponent(out T Component))
            return Component;
        else
        {
            //존재하지 않으면
            obj.SetActive(false);
            throw new Exception($"Componet not found");
        }
    }

    public static List<GameObject> GetAllPools(string tag)
    {
        if (!inst.poolDictionary.ContainsKey(tag))
            throw new Exception($"Pool with tag {tag} doesn't exist");

        //모든 오브젝트(비활성화 된 오브젝트까지)를 List<GameObject>로 반환
        return inst.spawnObjects.FindAll(x => x.name == tag);
    }

    public static List<T> GetAllPools<T>(string tag) where T : Component
    {
        List<GameObject> objects = GetAllPools(tag);

        if (!objects[0].TryGetComponent(out T component))
            throw new Exception($"Componet not found");

        //모든 GetComponent<T>로 반환받게 됨
        return objects.ConvertAll(x => x.GetComponent<T>());
    }

    public static void ReturnToPool(GameObject obj)
    {
        if (!inst.poolDictionary.ContainsKey(obj.name))
            throw new Exception($"Pool with tag {obj.name} doesn't exist");

        inst.poolDictionary[obj.name].Enqueue(obj);
    }

    // 우클릭으로 개수를 알아볼수 있음
    [ContextMenu("GetSpawnObjectsInfo")]
    void GetSpawnObjectsInfo()
    {
        foreach (var pool in pools)
        {
            // 이름과 태그가 같은것을 찾아서개수를 디버그로 띄워줌 
            int count = spawnObjects.FindAll(x => x.name == pool.tag).Count;
            Debug.Log($"{pool.tag} count : {count}");
        }
    }

    GameObject _SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        // 태그가 없으면 실행이 되지 않음
        if (!poolDictionary.ContainsKey(tag))
            throw new Exception($"Pool with tag {tag} doesn't exist");

        // 큐에 없으면 새로 추가
        Queue<GameObject> poolQueue = poolDictionary[tag];
        if (poolQueue.Count <= 0)
        {
            Pool pool = Array.Find(pools, x => x.tag == tag);
            var obj = CreateNewObject(pool.tag, pool.prefab);
            ArrangePool(obj);
        }

        // 큐에서 꺼내서 사용
        GameObject objectToSpawn = poolQueue.Dequeue();
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        return objectToSpawn;
    }

    void Start()
    {
        spawnObjects = new List<GameObject>();
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            poolDictionary.Add(pool.tag, new Queue<GameObject>());
            for (int i = 0; i < pool.size; i++)
            {
                var obj = CreateNewObject(pool.tag, pool.prefab);
                ArrangePool(obj);
            }
        }
    }
    GameObject CreateNewObject(string tag, GameObject prefab)
    {
        var obj = Instantiate(prefab, transform);
        obj.name = tag;
        obj.SetActive(false);   //비활성화시 ReturnToPool을 하므로 Enqueue가 됨
        return obj;
    }

    void ArrangePool(GameObject obj)
    {
        // 추가된 오므젝트 묶어서 정렬
        bool isFind = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            //childCount가 맨 마지막 이면 추가
            if (i == transform.childCount - 1)
            {
                obj.transform.SetSiblingIndex(i);
                spawnObjects.Insert(i, obj);
                break;
            }
            //맨 마지막이 아니면서 현재 이름과 오브젝트 이름이 같으면 isFind가 true가 됨
            else if (transform.GetChild(i).name == obj.name)
                isFind = true;
            //이름이 같지도 않고 맨마지막에 도달하지 않으면 Insert를 함
            else if (isFind)
            {
                obj.transform.SetSiblingIndex(i);
                spawnObjects.Insert(i, obj);
                break;
            }
        }
    }
}
