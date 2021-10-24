using CalApi.Core;

using PolyMap;

using UnityEngine;

using Object = UnityEngine.Object;
using PolyMap_ItemManager = PolyMap.ItemManager;

namespace CalApi.API.PolyMap {
    // ReSharper disable once UnusedType.Global
    public static class ItemManager {
        /*public static GameObject RegisterCustomItem(string name) {
            GameObject obj = Object.Instantiate(PolyMap_ItemManager.instance.itemPrefabs[0]);
            obj.hideFlags = HideFlags.HideInHierarchy;
            foreach(Transform trans in obj.transform) Object.Destroy(trans.gameObject);

            obj.name = name;

            obj.GetComponent<Item>().data.id = PolyMap_ItemManager.instance.itemPrefabs.Count;
            obj.tag = "Untagged";
            PolyMap_ItemManager.instance.itemPrefabs.Add(obj);
            return obj;
        }
        
        public static void CreateLightBulb(Vector3 translation) {
            GameObject obj = RegisterCustomItem("light bulb lmao, i wanna fucking die, why am i even doing this xd");
            obj.transform.position = new Vector3(translation.x, translation.y, 0f);
            obj.transform.localScale = Vector3.one * 0.75f;
            obj.transform.eulerAngles = new Vector3(0f, 0f, translation.z);
            
            GameObject rendererObj = new GameObject("draw the fucking light bulb");
            rendererObj.transform.SetParent(obj.transform);
            rendererObj.transform.localPosition = Vector3.zero;
            rendererObj.transform.localScale = Vector3.one;
            rendererObj.transform.localEulerAngles = Vector3.zero;
            SpriteRenderer renderer = rendererObj.AddComponent<SpriteRenderer>();
            renderer.sprite = CalApiPlugin.lightBulbSprite;
            renderer.rendererPriority = -51;
            
            GameObject smallRendererObj = new GameObject("alright, now the small part of the light bulb");
            smallRendererObj.transform.SetParent(rendererObj.transform);
            smallRendererObj.transform.localScale = Vector3.one * 0.5f;
            smallRendererObj.transform.localPosition = Vector3.down * 0.75f;
            smallRendererObj.transform.localEulerAngles = Vector3.zero;
            SpriteRenderer smallRenderer = smallRendererObj.AddComponent<SpriteRenderer>();
            smallRenderer.sprite = CalApiPlugin.lightBulbSprite;
            smallRenderer.color = Color.gray;
            smallRenderer.rendererPriority = -52;

            obj.AddComponent<LightBulb>();
        }*/
    }
}
