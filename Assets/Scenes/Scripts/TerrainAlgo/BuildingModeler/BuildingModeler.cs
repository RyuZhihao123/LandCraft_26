using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public enum BuildingType
{
	_none,
	_mansion,
	_skyscraper,
}





public class BuildingInfoBlockVersion
{
	public Vector3 baseCenter = Vector3.zero;
	public float scale = 1.0f;  // �����ǿ��ֱ��
	public float height = 5.0f;

	public BuildingType type = BuildingType._mansion;

	public bool hasRoof = false;
	public List<Vector3> botNodes = new List<Vector3>(); // after scaled.

	public BuildingInfoBlockVersion leftPart = null;
	public BuildingInfoBlockVersion rightPart = null;
	public BuildingInfoBlockVersion backPart = null;
	public BuildingInfoBlockVersion frontPart = null;
	public BuildingInfoBlockVersion topPart = null;

	public BuildingInfoBlockVersion() { }
	public BuildingInfoBlockVersion(BuildingType t) { type = t; }

	//public List<ComponentInfo> top_components = new List<ComponentInfo>();
}


public class BuildingModelerBlockVersion
{
	public (GameObject, Vector3) GetOneBldgInstance(BlockGeneration.Block block, Vector3 baseCenter, BuildingType type = BuildingType._mansion, float bldgHeightFactor = 1.0f, bool isUsingDecorations = true)
	{
		GameObject gameObject = new GameObject("Bldg");
		gameObject.transform.position = baseCenter;

		Material MaterialFacade = Resources.Load("Models/Buildings/MatBldgFacade") as Material;
		Material MaterialTopFace = Resources.Load("Models/Buildings/MatBldgTopface") as Material;
		Material MaterialRoof = Resources.Load("Models/Buildings/MatBldgRoof") as Material;
		Material MaterialHandRail = Resources.Load("Models/Buildings/HandRail/MatHandRail") as Material;
		Material MaterialPaddingBar = Resources.Load("Models/Buildings/PaddingBar/MatBackground") as Material;

		if (type == BuildingType._mansion)
		{
			int mainNum = Random.Range(1, 4 + 1);
			int topNum = Random.Range(1, 4 + 1);
			int roofNum = Random.Range(1, 4 + 1);
			Vector2 tilling = new Vector2(Random.Range(0.6f, 1.1f), Random.Range(0.6f, 1.1f));

			MaterialFacade.mainTextureScale = tilling;
			MaterialFacade.mainTexture = (Texture)Resources.Load(string.Format("Models/Buildings/Textures_Facade/mansion/{0}_base", mainNum));
			MaterialTopFace.mainTexture = (Texture)Resources.Load(string.Format("Models/Buildings/Textures_Topface/{0}_base", topNum));
			MaterialRoof.mainTexture = (Texture)Resources.Load(string.Format("Models/Buildings/Textures_Roof/{0}_base", roofNum));

			Texture temp_facade_mask = (Texture)Resources.Load(string.Format("Models/Buildings/Textures_Facade/mansion/{0}_mask", mainNum));
			Texture temp_top_mask = (Texture)Resources.Load(string.Format("Models/Buildings/Textures_Topface/{0}_mask", topNum));
			Texture temp_roof_mask = (Texture)Resources.Load(string.Format("Models/Buildings/Textures_Roof/{0}_mask", roofNum));
			MaterialFacade.SetTexture("_MaskMap", temp_facade_mask);
			MaterialTopFace.SetTexture("_MaskMap", temp_top_mask);
			MaterialRoof.SetTexture("_MaskMap", temp_roof_mask);

			Texture temp_facade_normal = (Texture)Resources.Load(string.Format("Models/Buildings/Textures_Facade/mansion/{0}_normal", mainNum));
			Texture temp_top_normal = (Texture)Resources.Load(string.Format("Models/Buildings/Textures_Topface/{0}_normal", topNum));
			Texture temp_roof_normal = (Texture)Resources.Load(string.Format("Models/Buildings/Textures_Roof/{0}_normal", roofNum));
			MaterialFacade.SetTexture("_NormalMap", temp_facade_normal);
			MaterialTopFace.SetTexture("_NormalMap", temp_top_normal);
			MaterialRoof.SetTexture("_NormalMap", temp_roof_normal);
		}
		else
		{
			int mainNum = Random.Range(1, 3 + 1);
			int topNum = Random.Range(1, 4 + 1);
			int roofNum = Random.Range(1, 4 + 1);
			Vector2 tilling = new Vector2(Random.Range(0.4f, 0.8f), Random.Range(0.4f, 0.8f));

			MaterialFacade.mainTextureScale = tilling;
			MaterialFacade.mainTexture = (Texture)Resources.Load(string.Format("Models/Buildings/Textures_Facade/skycraper/{0}_base", mainNum));
			MaterialTopFace.mainTexture = (Texture)Resources.Load(string.Format("Models/Buildings/Textures_Topface/{0}_base", topNum));
			MaterialRoof.mainTexture = (Texture)Resources.Load(string.Format("Models/Buildings/Textures_Roof/{0}_base", roofNum));

			Texture temp_facade_mask = (Texture)Resources.Load(string.Format("Models/Buildings/Textures_Facade/skycraper/{0}_mask", mainNum));
			Texture temp_top_mask = (Texture)Resources.Load(string.Format("Models/Buildings/Textures_Topface/{0}_mask", topNum));
			Texture temp_roof_mask = (Texture)Resources.Load(string.Format("Models/Buildings/Textures_Roof/{0}_mask", roofNum));
			MaterialFacade.SetTexture("_MaskMap", temp_facade_mask);
			MaterialTopFace.SetTexture("_MaskMap", temp_top_mask);
			MaterialRoof.SetTexture("_MaskMap", temp_roof_mask);

			Texture temp_facade_normal = (Texture)Resources.Load(string.Format("Models/Buildings/Textures_Facade/skycraper/{0}_normal", mainNum));
			Texture temp_top_normal = (Texture)Resources.Load(string.Format("Models/Buildings/Textures_Topface/{0}_normal", topNum));
			Texture temp_roof_normal = (Texture)Resources.Load(string.Format("Models/Buildings/Textures_Roof/{0}_normal", roofNum));
			MaterialFacade.SetTexture("_NormalMap", temp_facade_normal);
			MaterialTopFace.SetTexture("_NormalMap", temp_top_normal);
			MaterialRoof.SetTexture("_NormalMap", temp_roof_normal);
		}

		// Generate the entire building structure;
		BuildingInfoBlockVersion basePart = GetBldgStructure(type, bldgHeightFactor);

		// make vertices in counterclock-wise order.
		Vector3 centerPos;
		List<Vector3> botNodes;
		
		(botNodes, centerPos) = MakeAllNodesInCounterClockWise(block, 1.0f);  // �����size

	

		// ����ȫ��buildings�ĵ��涥��
		ComputerAllBotNodes(botNodes, basePart);


		// Create Facade urfaces;
		GameObject goFacade = ConstructBuildingFacades(block, basePart, MaterialFacade);
		goFacade.transform.parent = gameObject.transform;

		// Create a plain top face;
		if (BlockIsConvex(botNodes))
		{
			GameObject goTopface = ConstructBuildingTopFace_Convex(block, basePart, MaterialTopFace);
			goTopface.transform.parent = gameObject.transform;
		}
		else
		{
			GameObject goTopface = ConstructBuildingTopFace_Concave(block, basePart, MaterialTopFace);
			goTopface.transform.parent = gameObject.transform;
		}

		//// Distribute some components over the top face;
		//if (isUsingDecorations)
		//{
		//	GameObject goComponentsTopface = ConstructComponentsOnTopFace(basePart);
		//	goComponentsTopface.transform.parent = gameObject.transform;
		//}

		if (type == BuildingType._mansion)
		{
			if (botNodes.Count == 4)
			{
				GameObject goRoof = ConstructBuildingRoof(basePart, MaterialRoof);
				goRoof.transform.parent = gameObject.transform;
			}
			else
			{
				GameObject goRoof = ConstructBuildingRoof_Pyramid(basePart, MaterialRoof);
				goRoof.transform.parent = gameObject.transform;
			}
		}

		//GameObject goPaddingBar = ConstructPaddingBar(basePart, MaterialPaddingBar);
		//goPaddingBar.transform.parent = gameObject.transform;

		//GameObject goHandrailBar = ConstructHandRails(basePart, MaterialHandRail);
		//goHandrailBar.transform.parent = gameObject.transform;

		return (gameObject, centerPos);
	}

	public GameObject GetOnePointLight(Vector3 baseCenter, GlobalParams globalParams)
	{
		GameObject gameObject = new GameObject("BldgLight");
		Light pointLight = gameObject.AddComponent<Light>();
		pointLight.type = LightType.Point;
		pointLight.transform.position = baseCenter + new Vector3(
		Random.Range(-0.5f, 0.5f),
		Random.Range(15.0f, 30.5f),
		Random.Range(-0.5f, 0.5f)
		);

		float minRange = globalParams.point_light_min_range;
		float maxRange = globalParams.point_light_max_range;

		float lowerMinIntensity = globalParams.point_light_lower_min_intensity;
		float lowerMaxIntensity = globalParams.point_light_lower_max_intensity;

		float upperMinIntensity = globalParams.point_light_upper_min_intensity;
		float upperMaxIntensity = globalParams.point_light_upper_max_intensity;

		float intenProb = globalParams.point_light_upper_prob_intensity;

		HDAdditionalLightData hdLightData = gameObject.AddComponent<HDAdditionalLightData>();

		float intensity = 50000;
		intensity = Random.value > intenProb ?
		Random.Range(lowerMinIntensity, lowerMaxIntensity) :
		Random.Range(upperMinIntensity, upperMaxIntensity);

		hdLightData.intensity = intensity;

		hdLightData.lightUnit = LightUnit.Lumen; // 设定强度单位为 Lumen
												 // hdLightData.lightUnit = LightUnit.Lumen; // 使用Lumen作为强度单位
												 // hdLightData.SetIntensity(Random.Range(minIntensity, maxIntensity), LightUnit.Lumen); // 调整范围

		// pointLight.intensity = maxIntensity;
		pointLight.range = Random.Range(minRange, maxRange);
		hdLightData.range = pointLight.range;

		float randomValue = Random.value;

		if (randomValue <= 0.35f)
		{
			pointLight.color = new Color(235f / 255, 172f / 255, 172f / 255);
		}
		else if (randomValue <= 0.82f)
		{
			pointLight.color = Color.white;
		}
		else if (randomValue <= 0.88f)
		{
			pointLight.color = Color.white; //Color.red;
		}
		else if (randomValue <= 0.989f)
		{
			pointLight.color = Color.yellow;
		}
		else
		{
			pointLight.color = Color.blue;
		}

		return gameObject;
	}

	public (GameObject, Vector3) GetGardenArea(BlockGeneration.Block block, List<List<LocalVertexInfo>> map)
	{
		GameObject gameObject = new GameObject("GardenArea");

		Material MaterialTopFace = Resources.Load("Models/Buildings/MatBldgTopface") as Material;
		Material MaterialHandRail = Resources.Load("Models/Buildings/HandRail/MatHandRail") as Material;

		MaterialTopFace.mainTexture = (Texture)Resources.Load(
			"TerrainTextures/HDRPTextures/Surfaces/Uncut Grass_pjweO0/Textures/Albedo_2K__pjweO0");



		// Generate the entire building structure;

		float baseHeight = 0.5f;


		List<Vector3> botNodes;
		Vector3 centerPos;

		(botNodes, centerPos) = MakeAllNodesInCounterClockWise(block);

		centerPos = Vector3.zero;
		for (int i = 0; i < botNodes.Count; ++i)
		{

		}

		// Create a plain top face;
		//if (BlockIsConvex(botNodes))
		//{
		//	GameObject goTopface = ConstructBuildingTopFace_Convex(block, botNodes, baseHeight, MaterialTopFace);
		//	goTopface.transform.parent = gameObject.transform;
		//}
		//else
		//{
		//	GameObject goTopface = ConstructBuildingTopFace_Concave(block, botNodes, baseHeight, MaterialTopFace);
		//	goTopface.transform.parent = gameObject.transform;
		//}

		//// Distribute some components over the top face;
		//if (isUsingDecorations)
		//{
		//	GameObject goComponentsTopface = ConstructComponentsOnTopFace(basePart);
		//	goComponentsTopface.transform.parent = gameObject.transform;
		//}



		//GameObject goPaddingBar = ConstructPaddingBar(basePart, MaterialPaddingBar);
		//goPaddingBar.transform.parent = gameObject.transform;

		//GameObject goHandrailBar = ConstructHandRails(basePart, MaterialHandRail);
		//goHandrailBar.transform.parent = gameObject.transform;

		return (gameObject, centerPos);
	}

	private BuildingInfoBlockVersion GetBldgStructure(BuildingType bldgType, float bldgHeightFactor = 1.0f)
	{

		// First, create the base building part:
		BuildingInfoBlockVersion basePart = new BuildingInfoBlockVersion(bldgType);

		basePart.scale = 0.7f;
		basePart.baseCenter = Vector3.zero;



		if (bldgType == BuildingType._mansion)
			basePart.height = Random.Range(2.0f, 4.0f) * bldgHeightFactor;
		else
			basePart.height = Random.Range(7.0f, 9.0f) * bldgHeightFactor;


		if (bldgType == BuildingType._mansion)
		{
			//if (Random.Range(0.0f, 1.0f) > 0.3f)  // ǰ�� x+
			//{
			//    BuildingInfo subPart = new BuildingInfo(bldgType);
			//    subPart.scale.x = GetRandomInt(basePart.scale.x, min_scale, max_scale);
			//    subPart.scale.y = GetRandomInt(basePart.scale.y, min_scale, max_scale);
			//    subPart.scale.z = GetRandomInt(basePart.scale.z, min_scale, max_scale);

			//    subPart.baseCenter = basePart.baseCenter + new Vector3(subPart.scale.x / 2.0f, 0, 0) + new Vector3(basePart.scale.x / 2.0f, 0, 0);

			//    basePart.frontPart = subPart;
			//}
			if (Random.Range(0.0f, 1.0f) > 0.3f)  // ���� y+
			{
				BuildingInfoBlockVersion subPart = new BuildingInfoBlockVersion(bldgType);
				subPart.scale = Random.Range(0.4f, 0.6f);
				subPart.height = Random.Range(0.6f, 1.5f) * bldgHeightFactor;

				subPart.baseCenter = basePart.baseCenter + new Vector3(0, basePart.height, 0);  // ��

				subPart.hasRoof = Random.Range(0.0f, 1.0f) > 0.3f;

				basePart.topPart = subPart;  // ��
			}
			else
			{
				basePart.hasRoof = Random.Range(0.0f, 1.0f) > 0.5f;
			}
			//if (Random.Range(0.0f, 1.0f) > 0.3f)  // �Ҳ� z+
			//{
			//    BuildingInfo subPart = new BuildingInfo(bldgType);
			//    subPart.scale.x = GetRandomInt(basePart.scale.x, min_scale, max_scale);
			//    subPart.scale.y = GetRandomInt(basePart.scale.y, min_scale, max_scale);
			//    subPart.scale.z = GetRandomInt(basePart.scale.z, min_scale, max_scale);

			//    subPart.baseCenter = basePart.baseCenter + new Vector3(0, 0, subPart.scale.z / 2.0f) + new Vector3(0, 0, basePart.scale.z / 2.0f);

			//    basePart.rightPart = subPart;
			//}
		}

		return basePart;
	}

	private Vector3Int GetRandomScale(int x1, int x2, int y1, int y2, int z1, int z2)
	{
		return new Vector3Int(Random.Range(x1, x2 + 1), Random.Range(y1, y2 + 1), Random.Range(z1, z2 + 1));
	}

	private int GetRandomInt(int x, float min_scale, float max_scale)
	{
		int res = (int)Random.Range(x * min_scale, x * max_scale);

		if (res < 1)
			res = 1;
		return res;
	}


	//private GameObject ConstructComponentsOnTopFace(BuildingInfo basePart)
	//{
	//	Queue<BuildingInfo> queue = new Queue<BuildingInfo>();
	//	queue.Enqueue(basePart);

	//	GameObject finalObj = new GameObject("Components_Top");

	//	while (queue.Count != 0)
	//	{
	//		var part = queue.Dequeue();
	//		if (part.frontPart != null) queue.Enqueue(part.frontPart);
	//		if (part.backPart != null) queue.Enqueue(part.backPart);
	//		if (part.leftPart != null) queue.Enqueue(part.leftPart);
	//		if (part.rightPart != null) queue.Enqueue(part.rightPart);
	//		if (part.topPart != null) queue.Enqueue(part.topPart);


	//		// Skip with probablity - if it's a house without a top subpart.
	//		if (part.type == BuildingType._mansion && part.topPart == null && Random.Range(0.0f, 1.0f) < 0.3f)
	//			continue;

	//		if (part.topPart != null)
	//		{
	//			ComponentInfo c = new ComponentInfo();
	//			c.baseCenter = part.topPart.baseCenter;
	//			c.scale = part.topPart.scale;
	//			part.top_components.Add(c);
	//		}

	//		for (int i = 0; i < 10; i++)
	//		{
	//			string objname = string.Format("Models/Buildings/Components_topface/obj{0}", Random.Range(1, 3));
	//			if (AddOneComponentOnTopFace(finalObj, part, objname))
	//				break;
	//		}

	//		int pot_count = 0;

	//		for (int i = 0; i < 10; i++)
	//		{
	//			string objname = string.Format("Models/Buildings/Components_topface/pot{0}", Random.Range(1, 2));
	//			if (AddOneComponentOnTopFace(finalObj, part, objname))
	//				pot_count++;

	//			if (pot_count >= 3)
	//				break;
	//		}

	//	}

	//	return finalObj;
	//}

	//private bool AddOneComponentOnTopFace(GameObject finalObj, BuildingInfo part, string objname)
	//{
	//	float component_radius = 0.25f;
	//	Vector3 pos = part.baseCenter + new Vector3(
	//		Random.Range(-part.scale.x / 2.0f + component_radius, +part.scale.x / 2.0f - component_radius),
	//		part.scale.y,
	//		Random.Range(-part.scale.z / 2.0f + component_radius, +part.scale.z / 2.0f - component_radius));

	//	foreach (var c in part.top_components)
	//	{
	//		float dx = (c.baseCenter.x) - (pos.x);
	//		float dz = (c.baseCenter.z) - (pos.z);

	//		float minDeltaX = c.scale.x / 2.0f + component_radius;
	//		float minDeltaZ = c.scale.z / 2.0f + component_radius;

	//		if (Mathf.Abs(dx) <= minDeltaX && Mathf.Abs(dz) <= minDeltaZ)
	//			return false;
	//	}

	//	GameObject defaultObj = (GameObject)Resources.Load(objname);
	//	GameObject instancedObj = GameObject.Instantiate(defaultObj, pos, Quaternion.identity);
	//	instancedObj.transform.parent = finalObj.transform;

	//	// ������ײ��
	//	ComponentInfo comp = new ComponentInfo();
	//	comp.baseCenter = pos;
	//	comp.scale = new Vector3(component_radius * 2.0f, 1, component_radius * 2.0f);
	//	part.top_components.Add(comp);

	//	return true;
	//}


	//private GameObject ConstructHandRails(BuildingInfo basePart, Material mat)
	//{
	//	Queue<BuildingInfo> queue = new Queue<BuildingInfo>();
	//	queue.Enqueue(basePart);

	//	GameObject defaultRail = (GameObject)Resources.Load("Models/Buildings/HandRail/HandRail");
	//	GameObject finalObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
	//	//GameObject finalObj = new GameObject();
	//	finalObj.name = "HandRail";

	//	List<CombineInstance> combines = new List<CombineInstance>();
	//	List<GameObject> temp_objects = new List<GameObject>();

	//	while (queue.Count != 0)
	//	{
	//		var part = queue.Dequeue();
	//		if (part.frontPart != null) queue.Enqueue(part.frontPart);
	//		if (part.backPart != null) queue.Enqueue(part.backPart);
	//		if (part.leftPart != null) queue.Enqueue(part.leftPart);
	//		if (part.rightPart != null) queue.Enqueue(part.rightPart);
	//		if (part.topPart != null) queue.Enqueue(part.topPart);

	//		if (part.topPart == null && basePart.type == BuildingType._mansion)
	//			continue;

	//		{  // +x
	//			Vector3 startPos = part.baseCenter + new Vector3(+part.scale.x / 2.0f - 0.5f, part.scale.y, +part.scale.z / 2.0f);
	//			Vector3 dir = new Vector3(-1, 0, 0);
	//			for (int i = 0; i < part.scale.x; ++i)
	//			{
	//				Vector3 pos = startPos + i * dir;
	//				GameObject rail = GameObject.Instantiate(defaultRail, pos, Quaternion.identity);

	//				temp_objects.Add(rail);
	//				CombineInstance comb = new CombineInstance();
	//				comb.mesh = rail.GetComponent<MeshFilter>().mesh;
	//				comb.transform = rail.transform.localToWorldMatrix;
	//				combines.Add(comb);
	//			}
	//		}

	//		{  // -x
	//			Vector3 startPos = part.baseCenter + new Vector3(+part.scale.x / 2.0f - 0.5f, part.scale.y, -part.scale.z / 2.0f);
	//			Vector3 dir = new Vector3(-1, 0, 0);
	//			for (int i = 0; i < part.scale.x; ++i)
	//			{
	//				Vector3 pos = startPos + i * dir;
	//				GameObject rail = GameObject.Instantiate(defaultRail, pos, Quaternion.identity);

	//				temp_objects.Add(rail);
	//				CombineInstance comb = new CombineInstance();
	//				comb.mesh = rail.GetComponent<MeshFilter>().mesh;
	//				comb.transform = rail.transform.localToWorldMatrix;
	//				combines.Add(comb);
	//			}
	//		}

	//		{  // +z
	//			Vector3 startPos = part.baseCenter + new Vector3(+part.scale.x / 2.0f, part.scale.y, +part.scale.z / 2.0f - 0.5f);
	//			Vector3 dir = new Vector3(0, 0, -1);
	//			for (int i = 0; i < part.scale.z; ++i)
	//			{
	//				Vector3 pos = startPos + i * dir;
	//				GameObject rail = GameObject.Instantiate(defaultRail, pos, Quaternion.identity);
	//				rail.transform.localRotation = Quaternion.AngleAxis(90.0f, Vector3.up);
	//				//rail.transform.parent = finalObj.transform;

	//				temp_objects.Add(rail);
	//				CombineInstance comb = new CombineInstance();
	//				comb.mesh = rail.GetComponent<MeshFilter>().mesh;
	//				comb.transform = rail.transform.localToWorldMatrix;
	//				combines.Add(comb);

	//			}
	//		}
	//		{  // -z
	//			Vector3 startPos = part.baseCenter + new Vector3(-part.scale.x / 2.0f, part.scale.y, +part.scale.z / 2.0f - 0.5f);
	//			Vector3 dir = new Vector3(0, 0, -1);
	//			for (int i = 0; i < part.scale.z; ++i)
	//			{
	//				Vector3 pos = startPos + i * dir;
	//				GameObject rail = GameObject.Instantiate(defaultRail, pos, Quaternion.identity);
	//				rail.transform.localRotation = Quaternion.AngleAxis(90.0f, Vector3.up);

	//				temp_objects.Add(rail);
	//				CombineInstance comb = new CombineInstance();
	//				comb.mesh = rail.GetComponent<MeshFilter>().mesh;
	//				comb.transform = rail.transform.localToWorldMatrix;
	//				combines.Add(comb);
	//			}
	//		}


	//	}
	//	Mesh mesh = new Mesh();
	//	mesh.CombineMeshes(combines.ToArray(), true);
	//	//Debug.Log(mesh.vertices.Length);

	//	finalObj.GetComponent<MeshFilter>().mesh = mesh;
	//	finalObj.GetComponent<MeshRenderer>().material = mat;

	//	// clear temp gameobject buffers;
	//	foreach (var go in temp_objects)
	//		UnityEngine.GameObject.Destroy(go);

	//	return finalObj;
	//}

	//private GameObject ConstructPaddingBar(BuildingInfo basePart, Material mat)
	//{
	//	Queue<BuildingInfo> queue = new Queue<BuildingInfo>();
	//	queue.Enqueue(basePart);

	//	GameObject defaultBar = (GameObject)Resources.Load("Models/Buildings/PaddingBar/PaddingBar");
	//	GameObject finalBarObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
	//	finalBarObj.name = "PaddingBar";

	//	List<CombineInstance> combines = new List<CombineInstance>();
	//	List<GameObject> temp_objects = new List<GameObject>();

	//	while (queue.Count != 0)
	//	{
	//		var part = queue.Dequeue();
	//		if (part.frontPart != null) queue.Enqueue(part.frontPart);
	//		if (part.backPart != null) queue.Enqueue(part.backPart);
	//		if (part.leftPart != null) queue.Enqueue(part.leftPart);
	//		if (part.rightPart != null) queue.Enqueue(part.rightPart);
	//		if (part.topPart != null) queue.Enqueue(part.topPart);

	//		Vector3 pos1 = part.baseCenter + new Vector3(+part.scale.x / 2.0f, 0, +part.scale.z / 2.0f);
	//		Vector3 pos2 = part.baseCenter + new Vector3(+part.scale.x / 2.0f, 0, -part.scale.z / 2.0f);
	//		Vector3 pos3 = part.baseCenter + new Vector3(-part.scale.x / 2.0f, 0, +part.scale.z / 2.0f);
	//		Vector3 pos4 = part.baseCenter + new Vector3(-part.scale.x / 2.0f, 0, -part.scale.z / 2.0f);
	//		Vector3 scale = new Vector3(1, part.scale.y * 0.96f, 1);

	//		GameObject bar1 = GameObject.Instantiate(defaultBar, pos1, Quaternion.identity);
	//		GameObject bar2 = GameObject.Instantiate(defaultBar, pos2, Quaternion.identity);
	//		GameObject bar3 = GameObject.Instantiate(defaultBar, pos3, Quaternion.identity);
	//		GameObject bar4 = GameObject.Instantiate(defaultBar, pos4, Quaternion.identity);

	//		bar1.transform.localScale = bar2.transform.localScale = bar3.transform.localScale
	//			= bar4.transform.localScale = scale;


	//		CombineInstance comb1 = new CombineInstance(); CombineInstance comb2 = new CombineInstance();
	//		CombineInstance comb3 = new CombineInstance(); CombineInstance comb4 = new CombineInstance();

	//		comb1.mesh = bar1.GetComponent<MeshFilter>().mesh; comb2.mesh = bar2.GetComponent<MeshFilter>().mesh;
	//		comb3.mesh = bar3.GetComponent<MeshFilter>().mesh; comb4.mesh = bar4.GetComponent<MeshFilter>().mesh;

	//		comb1.transform = bar1.transform.localToWorldMatrix; comb2.transform = bar2.transform.localToWorldMatrix;
	//		comb3.transform = bar3.transform.localToWorldMatrix; comb4.transform = bar4.transform.localToWorldMatrix;

	//		combines.Add(comb1); combines.Add(comb2); combines.Add(comb3); combines.Add(comb4);

	//		temp_objects.Add(bar1); temp_objects.Add(bar2); temp_objects.Add(bar3); temp_objects.Add(bar4);

	//	}

	//	Mesh mesh = new Mesh();
	//	mesh.CombineMeshes(combines.ToArray(), true);
	//	Debug.Log(mesh.vertices.Length);

	//	finalBarObj.GetComponent<MeshFilter>().mesh = mesh;
	//	finalBarObj.GetComponent<MeshRenderer>().material = mat;

	//	// clear temp gameobject buffers;
	//	foreach (var go in temp_objects)
	//		UnityEngine.GameObject.Destroy(go);

	//	return finalBarObj;
	//}


	private (List<Vector3>, Vector3) MakeAllNodesInCounterClockWise(BlockGeneration.Block block, float scale = 1.0f)
	{
		List<Vector3> nodes = new List<Vector3>();
		Vector3 centerPos = Vector3.zero;
		for (int i = 0; i < block.Nodes.Count; ++i)
		{
			Vector3 v = new Vector3(block.Nodes[i].X, 0, block.Nodes[i].Y);
			nodes.Add(v);
			centerPos += v;
		}

		centerPos /= nodes.Count;

		for (int i = 0; i < nodes.Count; ++i)
		{
			nodes[i] -= centerPos;
		}

		for (int i = 0; i < nodes.Count; ++i)
		{
			nodes[i] *= scale;
		}

		if (!IsCounterClockwise(nodes))   // �����е�blocks��������ʱ��˳��
		{
			nodes.Reverse();
		}

		return (nodes, centerPos);
	}


	private void ComputerAllBotNodes(List<Vector3> botNodes, BuildingInfoBlockVersion basePart)
	{
		Queue<BuildingInfoBlockVersion> queue = new Queue<BuildingInfoBlockVersion>();
		queue.Enqueue(basePart);

		while (queue.Count != 0)
		{
			var curPart = queue.Dequeue();

			for (int i = 0; i < botNodes.Count; ++i)
			{
				curPart.botNodes.Add(botNodes[i] * curPart.scale);
			}

			if (curPart.topPart != null) queue.Enqueue(curPart.topPart);
		}
	}



	private GameObject ConstructBuildingFacades(BlockGeneration.Block block, BuildingInfoBlockVersion basePart, Material mat)
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		gameObject.name = "facades";
		gameObject.GetComponent<MeshRenderer>().material.CopyPropertiesFromMaterial(mat);


		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uv = new List<Vector2>();
		List<int> triangles = new List<int>();
		int count = 0;


		Queue<BuildingInfoBlockVersion> queue = new Queue<BuildingInfoBlockVersion>();
		queue.Enqueue(basePart);

		while (queue.Count != 0)
		{
			var curPart = queue.Dequeue();

			for (int i = 0; i < curPart.botNodes.Count; ++i)
			{
				Vector3 v0 = curPart.baseCenter + curPart.botNodes[i + 0];
				Vector3 v1 = curPart.baseCenter + curPart.botNodes[(i + 1) % curPart.botNodes.Count];
				Vector3 v2 = v1 + new Vector3(0.0f, curPart.height, 0.0f);
				Vector3 v3 = v0 + new Vector3(0.0f, curPart.height, 0.0f);

				int len = (int)((v1 - v0).magnitude) / 2;

				Vector2 t0 = new Vector2(0, 0);
				Vector2 t1 = new Vector2(len, 0);
				Vector2 t2 = new Vector2(len, (int)(curPart.height / 3));
				Vector2 t3 = new Vector2(0, (int)(curPart.height / 3));

				vertices.Add(v0); vertices.Add(v1); vertices.Add(v2); vertices.Add(v3);
				uv.Add(t0); uv.Add(t1); uv.Add(t2); uv.Add(t3);

				triangles.Add(count + 0); triangles.Add(count + 2); triangles.Add(count + 1);
				triangles.Add(count + 2); triangles.Add(count + 0); triangles.Add(count + 3);
				count += 4;

			}

			if (curPart.topPart != null) queue.Enqueue(curPart.topPart);
		}





		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.uv = uv.ToArray();
		mesh.triangles = triangles.ToArray();

		mesh.RecalculateNormals();
		mesh.RecalculateTangents();

		gameObject.GetComponent<MeshFilter>().mesh = mesh;

		return gameObject;
	}


	private GameObject ConstructBuildingTopFace_Convex(BlockGeneration.Block block, BuildingInfoBlockVersion basePart, Material mat)
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		gameObject.name = "Topface";
		gameObject.GetComponent<MeshRenderer>().material.CopyPropertiesFromMaterial(mat);


		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uv = new List<Vector2>();
		List<int> triangles = new List<int>();
		int count = 0;


		Queue<BuildingInfoBlockVersion> queue = new Queue<BuildingInfoBlockVersion>();
		queue.Enqueue(basePart);

		while (queue.Count != 0)
		{
			var curPart = queue.Dequeue();

			float minX, minZ, maxX, maxZ;
			minX = minZ = +999999.0f;
			maxX = maxZ = -999999.9f;


			foreach (var node in curPart.botNodes)
			{
				minX = Mathf.Min(node.x, minX);
				minZ = Mathf.Min(node.z, minZ);

				maxX = Mathf.Max(node.x, maxX);
				maxZ = Mathf.Max(node.z, maxZ);
			}

			for (int i = 2; i < curPart.botNodes.Count; i++)
			{
				//For gizmos, we make the Vectors like this now, later Y goes to Z (Y is up)
				Vector3 v0 = curPart.baseCenter + curPart.botNodes[0] + new Vector3(0, curPart.height, 0);
				Vector3 v1 = curPart.baseCenter + curPart.botNodes[i - 1] + new Vector3(0, curPart.height, 0);
				Vector3 v2 = curPart.baseCenter + curPart.botNodes[i] + new Vector3(0, curPart.height, 0);

				Vector2 t0 = new Vector2((v0.x - minX) / (maxX - minX), (v0.z - minZ) / (maxZ - minZ));
				Vector2 t1 = new Vector2((v1.x - minX) / (maxX - minX), (v1.z - minZ) / (maxZ - minZ));
				Vector2 t2 = new Vector2((v2.x - minX) / (maxX - minX), (v2.z - minZ) / (maxZ - minZ));

				vertices.Add(v0); vertices.Add(v1); vertices.Add(v2);
				uv.Add(t0); uv.Add(t1); uv.Add(t2);

				triangles.Add(count + 0); triangles.Add(count + 2); triangles.Add(count + 1);
				count += 3;

			}

			if (curPart.topPart != null) queue.Enqueue(curPart.topPart);
		}



		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.uv = uv.ToArray();
		mesh.triangles = triangles.ToArray();

		mesh.RecalculateNormals();
		mesh.RecalculateTangents();

		gameObject.GetComponent<MeshFilter>().mesh = mesh;

		return gameObject;
	}




	private GameObject ConstructBuildingTopFace_Concave(BlockGeneration.Block block, BuildingInfoBlockVersion basePart, Material mat)
	{

		List<Vector3> mesh_vertices = new List<Vector3>();
		List<Vector2> mesh_uv = new List<Vector2>();
		List<int> mesh_triangles = new List<int>();
		int count = 0;

		Queue<BuildingInfoBlockVersion> queue = new Queue<BuildingInfoBlockVersion>();
		queue.Enqueue(basePart);

		while (queue.Count != 0)
		{
			var curPart = queue.Dequeue();
			if (curPart.topPart != null) queue.Enqueue(curPart.topPart);

			//Step 0. Check if the block stores the vertexes counter clockwise or not
			//bool counterClockwise = IsCounterClockwise(botNodes);

			//The list with triangles, that the ear clipping algorithm will generate
			List<Triangle> triangles = new List<Triangle>();

			//Step 1. Store the vertices in a list and we also need to know the next and prev vertex
			List<Vertex> vertices = new List<Vertex>();
			for (int i = 0; i < curPart.botNodes.Count; i++)
				vertices.Add(new Vertex(new Vector3(curPart.botNodes[i].x, 0f, curPart.botNodes[i].z)));

			//Find the next and previous vertex
			vertices[0].PrevVertex = vertices[vertices.Count - 1];
			vertices[0].NextVertex = vertices[1];
			vertices[vertices.Count - 1].PrevVertex = vertices[vertices.Count - 2];
			vertices[vertices.Count - 1].NextVertex = vertices[0];

			for (int i = 1; i < vertices.Count - 1; i++)
			{
				vertices[i].PrevVertex = vertices[i - 1];
				vertices[i].NextVertex = vertices[i + 1];
			}

			//Step 2. Find the reflex (concave) and convex vertices, and ear vertices
			for (int i = 0; i < vertices.Count; i++)
				CheckIfReflexOrConvex(vertices[i]);

			//Have to find the ears after we have found if the vertex is reflex or convex
			List<Vertex> earVertices = new List<Vertex>();

			for (int i = 0; i < vertices.Count; i++)
				IsVertexEar(vertices[i], vertices, earVertices);

			//Step 3. Triangulate!
			while (true)
			{
				//This means we have just one triangle left
				if (vertices.Count == 3)
				{
					//The final triangle
					triangles.Add(new Triangle(vertices[0].Position, vertices[0].PrevVertex.Position, vertices[0].NextVertex.Position));
					break;
				}

				if (earVertices.Count == 0)
				{
					Debug.Log("earVertices not found, Triangulation failed.");
					break;
				}

				//Make a triangle of the first ear
				Vertex earVertex = earVertices[0];

				Vertex earVertexPrev = earVertex.PrevVertex;
				Vertex earVertexNext = earVertex.NextVertex;

				Triangle newTriangle = new Triangle(earVertex.Position, earVertexPrev.Position, earVertexNext.Position);

				triangles.Add(newTriangle);

				//Remove the vertex from the lists
				earVertices.Remove(earVertex);
				vertices.Remove(earVertex);

				//Update the previous vertex and next vertex
				earVertexPrev.NextVertex = earVertexNext;
				earVertexNext.PrevVertex = earVertexPrev;

				//...see if we have found a new ear by investigating the two vertices that was part of the ear
				CheckIfReflexOrConvex(earVertexPrev);
				CheckIfReflexOrConvex(earVertexNext);

				earVertices.Remove(earVertexPrev);
				earVertices.Remove(earVertexNext);

				IsVertexEar(earVertexPrev, vertices, earVertices);
				IsVertexEar(earVertexNext, vertices, earVertices);
			}


			//Step4 insert the triangles inside the BlockMesh
			float minX, minZ, maxX, maxZ;
			minX = minZ = +999999.0f;
			maxX = maxZ = -999999.9f;

			foreach (var node in curPart.botNodes)
			{
				minX = Mathf.Min(node.x, minX);
				minZ = Mathf.Min(node.z, minZ);

				maxX = Mathf.Max(node.x, maxX);
				maxZ = Mathf.Max(node.z, maxZ);
			}

			foreach (Triangle tri in triangles)
			{

				Vector3 v0 = curPart.baseCenter + new Vector3(tri.A.x, tri.A.y, tri.A.z) + new Vector3(0, curPart.height, 0); //We want to draw the blockMeshes as gizmos right now! (up is z right now in the 2d model)
				Vector3 v1 = curPart.baseCenter + new Vector3(tri.B.x, tri.B.y, tri.B.z) + new Vector3(0, curPart.height, 0);
				Vector3 v2 = curPart.baseCenter + new Vector3(tri.C.x, tri.C.y, tri.C.z) + new Vector3(0, curPart.height, 0);

				Vector2 t0 = new Vector2((v0.x - minX) / (maxX - minX), (v0.z - minZ) / (maxZ - minZ));
				Vector2 t1 = new Vector2((v1.x - minX) / (maxX - minX), (v1.z - minZ) / (maxZ - minZ));
				Vector2 t2 = new Vector2((v2.x - minX) / (maxX - minX), (v2.z - minZ) / (maxZ - minZ));

				mesh_vertices.Add(v0); mesh_vertices.Add(v1); mesh_vertices.Add(v2);
				mesh_uv.Add(t0); mesh_uv.Add(t1); mesh_uv.Add(t2);

				mesh_triangles.Add(count + 0); mesh_triangles.Add(count + 1); mesh_triangles.Add(count + 2);
				count += 3;

			}

		}

		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		gameObject.name = "Topface";
		gameObject.GetComponent<MeshRenderer>().material.CopyPropertiesFromMaterial(mat);

		Mesh mesh = new Mesh();
		mesh.vertices = mesh_vertices.ToArray();
		mesh.uv = mesh_uv.ToArray();
		mesh.triangles = mesh_triangles.ToArray();

		mesh.RecalculateNormals();
		mesh.RecalculateTangents();

		gameObject.GetComponent<MeshFilter>().mesh = mesh;

		return gameObject;


	}



	private GameObject ConstructBuildingRoof(BuildingInfoBlockVersion basePart, Material mat)
	{

		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		gameObject.name = "Roof";
		gameObject.GetComponent<MeshRenderer>().material.CopyPropertiesFromMaterial(mat);



		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uv = new List<Vector2>();
		List<int> triangles = new List<int>();
		int count = 0;

		Queue<BuildingInfoBlockVersion> queue = new Queue<BuildingInfoBlockVersion>();
		queue.Enqueue(basePart);

		while (queue.Count != 0)
		{
			var curPart = queue.Dequeue();
			if (curPart.topPart != null) queue.Enqueue(curPart.topPart);

			if (curPart.hasRoof == false)
				continue;
			//
			Vector3 roofDir = Vector3.left;
			float roofLen = float.MinValue;
			Vector3 A, B, C, D;
			A = B = C = D = Vector3.zero;
			// B            C
			//   \        /
			//    E------F
			//   /        \
			// A            D
			Vector3 roofCenter = Vector3.zero;
			{
				for (int i = 0; i < curPart.botNodes.Count; ++i)
				{
					Vector3 dir = curPart.botNodes[(i + 1) % curPart.botNodes.Count] - curPart.botNodes[i];  // C - B
					float len = dir.magnitude;

					roofCenter += curPart.botNodes[i];

					if (len > roofLen)
					{
						roofLen = len;
						roofDir = dir.normalized;

						int pre_id = (i - 1) % curPart.botNodes.Count;
						pre_id = pre_id < 0 ? curPart.botNodes.Count + pre_id : pre_id;

						A = curPart.botNodes[pre_id];
						B = curPart.botNodes[i];
						C = curPart.botNodes[(i + 1) % curPart.botNodes.Count];
						D = curPart.botNodes[(i + 2) % curPart.botNodes.Count];

					}
				}
				roofCenter /= curPart.botNodes.Count;
			}


			// �Ѹ߶ȼ���
			float roofHeight = 1.5f;
			float z_factor = 0.5f;
			// �ʵ�����һ��roofLen
			roofLen *= 0.35f;
			A = curPart.baseCenter + A + new Vector3(0, curPart.height, 0);
			B = curPart.baseCenter + B + new Vector3(0, curPart.height, 0);
			C = curPart.baseCenter + C + new Vector3(0, curPart.height, 0);
			D = curPart.baseCenter + D + new Vector3(0, curPart.height, 0);
			Vector3 E = curPart.baseCenter + roofCenter - roofLen * roofDir + new Vector3(0, roofHeight + curPart.height, 0);
			Vector3 F = curPart.baseCenter + roofCenter + roofLen * roofDir + new Vector3(0, roofHeight + curPart.height, 0);


			{
				// front (+x)
				{
					// rt rb lb lt
					Vector3 v0 = E;
					Vector3 v1 = A;
					Vector3 v2 = D;
					Vector3 v3 = F;
					v1 = v0 + 1.03f * (v1 - v0);
					v2 = v3 + 1.03f * (v2 - v3);

					Vector2 t0 = new Vector2(1, 1);
					Vector2 t1 = new Vector2(1, 0);
					Vector2 t2 = new Vector2(0, 0);
					Vector2 t3 = new Vector2(0, 1);


					vertices.Add(v0); vertices.Add(v1); vertices.Add(v2); vertices.Add(v3);
					uv.Add(t0); uv.Add(t1); uv.Add(t2); uv.Add(t3);
					triangles.Add(count + 0); triangles.Add(count + 1); triangles.Add(count + 2);
					triangles.Add(count + 2); triangles.Add(count + 3); triangles.Add(count + 0);
					count += 4;
				}
				// right (+z)
				{
					// rt rb lb lt
					Vector3 v0 = F;
					Vector3 v1 = D;
					Vector3 v2 = C;
					Vector3 v3 = F;
					v1 = v0 + 1.03f * (v1 - v0);
					v2 = v3 + 1.03f * (v2 - v3);

					Vector2 t0 = new Vector2(1, 1);
					Vector2 t1 = new Vector2(1, 0);
					Vector2 t2 = new Vector2(0, 0);
					Vector2 t3 = new Vector2(0, 1);


					vertices.Add(v0); vertices.Add(v1); vertices.Add(v2); vertices.Add(v3);
					uv.Add(t0); uv.Add(t1); uv.Add(t2); uv.Add(t3);
					triangles.Add(count + 0); triangles.Add(count + 1); triangles.Add(count + 2);
					triangles.Add(count + 2); triangles.Add(count + 3); triangles.Add(count + 0);
					count += 4;
				}
				// back (-x)
				{
					// rt rb lb lt
					Vector3 v0 = F;
					Vector3 v1 = C;
					Vector3 v2 = B;
					Vector3 v3 = E;
					v1 = v0 + 1.03f * (v1 - v0);
					v2 = v3 + 1.03f * (v2 - v3);

					Vector2 t0 = new Vector2(1, 1);
					Vector2 t1 = new Vector2(1, 0);
					Vector2 t2 = new Vector2(0, 0);
					Vector2 t3 = new Vector2(0, 1);


					vertices.Add(v0); vertices.Add(v1); vertices.Add(v2); vertices.Add(v3);
					uv.Add(t0); uv.Add(t1); uv.Add(t2); uv.Add(t3);
					triangles.Add(count + 0); triangles.Add(count + 1); triangles.Add(count + 2);
					triangles.Add(count + 2); triangles.Add(count + 3); triangles.Add(count + 0);
					count += 4;
				}
				// left (-z)
				{
					// rt rb lb lt
					Vector3 v0 = E;
					Vector3 v1 = B;
					Vector3 v2 = A;
					Vector3 v3 = E;
					v1 = v0 + 1.03f * (v1 - v0);
					v2 = v3 + 1.03f * (v2 - v3);

					Vector2 t0 = new Vector2(1, 1);
					Vector2 t1 = new Vector2(1, 0);
					Vector2 t2 = new Vector2(0, 0);
					Vector2 t3 = new Vector2(0, 1);


					vertices.Add(v0); vertices.Add(v1); vertices.Add(v2); vertices.Add(v3);
					uv.Add(t0); uv.Add(t1); uv.Add(t2); uv.Add(t3);
					triangles.Add(count + 0); triangles.Add(count + 1); triangles.Add(count + 2);
					triangles.Add(count + 2); triangles.Add(count + 3); triangles.Add(count + 0);
					count += 4;
				}
			}
		}



		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.uv = uv.ToArray();
		mesh.triangles = triangles.ToArray();

		mesh.RecalculateNormals();
		mesh.RecalculateTangents();

		gameObject.GetComponent<MeshFilter>().mesh = mesh;

		return gameObject;
	}


	private GameObject ConstructBuildingRoof_Pyramid(BuildingInfoBlockVersion basePart, Material mat)
	{

		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		gameObject.name = "Roof";
		gameObject.GetComponent<MeshRenderer>().material.CopyPropertiesFromMaterial(mat);
		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uv = new List<Vector2>();
		List<int> triangles = new List<int>();
		int count = 0;

		Queue<BuildingInfoBlockVersion> queue = new Queue<BuildingInfoBlockVersion>();
		queue.Enqueue(basePart);

		while (queue.Count != 0)
		{
			var curPart = queue.Dequeue();
			if (curPart.topPart != null) queue.Enqueue(curPart.topPart);

			if (curPart.hasRoof == false)
				continue;

			Vector3 roofCenter = Vector3.zero;
			{
				for (int i = 0; i < curPart.botNodes.Count; ++i)
				{
					roofCenter += curPart.botNodes[i];
				}
				roofCenter /= curPart.botNodes.Count;
			}

			float roofHeight = 1.8f;
			roofCenter = roofCenter + new Vector3(0, roofHeight + curPart.height, 0);

			// �Ѹ߶ȼ���
			for (int i = 0; i < curPart.botNodes.Count; ++i)
			{
				// rt rb lb lt
				Vector3 v0 = curPart.baseCenter + curPart.botNodes[(i + 1) % curPart.botNodes.Count] + new Vector3(0, curPart.height, 0);
				Vector3 v1 = curPart.baseCenter + curPart.botNodes[i] + new Vector3(0, curPart.height, 0);
				Vector3 v2 = curPart.baseCenter + roofCenter;
				v0 = v2 + 1.03f * (v0 - v2);
				v1 = v2 + 1.03f * (v1 - v2);

				Vector2 t0 = new Vector2(0, 0);
				Vector2 t1 = new Vector2(1, 0);
				Vector2 t2 = new Vector2(0.5f, 1);



				vertices.Add(v0); vertices.Add(v1); vertices.Add(v2);
				uv.Add(t0); uv.Add(t1); uv.Add(t2);
				triangles.Add(count + 0); triangles.Add(count + 1); triangles.Add(count + 2);
				count += 3;
			}
		}





		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.uv = uv.ToArray();
		mesh.triangles = triangles.ToArray();

		mesh.RecalculateNormals();
		mesh.RecalculateTangents();

		gameObject.GetComponent<MeshFilter>().mesh = mesh;

		return gameObject;
	}


	/// helper
	private bool BlockIsConvex(List<Vector3> block)
	{
		// For each set of three adjacent points find the cross product. 
		// If the sign of all the cross products is the same, the angles are all positive or negative (depending on the order in which we visit them) so the polygon is convex.

		bool gotNegative = false;
		bool gotPositive = false;

		int numPoints = block.Count;

		int B, C;

		for (int A = 0; A < numPoints; A++)
		{
			B = (A + 1) % numPoints;
			C = (B + 1) % numPoints;

			float crossProduct = CrossProductLength(
					block[A].x, block[A].z,
					block[B].x, block[B].z,
					block[C].x, block[C].z);

			if (crossProduct < 0)
			{
				gotNegative = true;
			}
			else if (crossProduct > 0)
			{
				gotPositive = true;
			}

			if (gotNegative && gotPositive) return false;
		}

		return true;
	}

	private float CrossProductLength(float Ax, float Ay, float Bx, float By, float Cx, float Cy)
	{
		// Get the vectors' coordinates.
		float BAx = Ax - Bx;
		float BAy = Ay - By;
		float BCx = Cx - Bx;
		float BCy = Cy - By;

		return (BAx * BCy - BAy * BCx);
	}

	private bool IsCounterClockwise(List<Vector3> block)
	{
		float sum = 0.0f;
		for (int i = 0; i < block.Count; i++)
		{
			if (i == block.Count - 1) sum += (block[i].x * block[0].z - block[i].z * block[0].x); //Last node reached
			else sum += (block[i].x * block[i + 1].z - block[i].z * block[i + 1].x);
		}
		return sum >= 0;
	}

	class Triangle
	{
		public Vector3 A { get; set; }
		public Vector3 B { get; set; }
		public Vector3 C { get; set; }

		public Triangle(Vector3 a, Vector3 b, Vector3 c)
		{
			A = a;
			B = b;
			C = c;
		}
	}

	public class Vertex
	{
		public Vector3 Position;

		//The previous and next vertex this vertex is attached to
		public Vertex PrevVertex;
		public Vertex NextVertex;

		//Properties this vertex may have
		//Reflex is concave
		public bool IsReflex;
		public bool IsConvex;
		//public bool isEar;

		public Vertex(Vector3 position)
		{
			this.Position = position;
		}

		//Get 2d pos of this vertex
		public Vector2 GetPos2D_XZ()
		{
			Vector2 pos_2d_xz = new Vector2(Position.x, Position.z);

			return pos_2d_xz;
		}
	}

	private void CheckIfReflexOrConvex(Vertex v)
	{
		v.IsReflex = false;
		v.IsConvex = false;

		//This is a reflex vertex if its triangle is oriented clockwise
		Vector2 a = v.PrevVertex.GetPos2D_XZ();
		Vector2 b = v.GetPos2D_XZ();
		Vector2 c = v.NextVertex.GetPos2D_XZ();

		if (IsTriangleOrientedClockwise(a, b, c))
		{
			v.IsReflex = true;
		}
		else
		{
			v.IsConvex = true;
		}
	}

	private bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
	{
		bool isClockWise = true;

		float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

		if (determinant > 0f)
		{
			isClockWise = false;
		}

		return isClockWise;
	}

	//Check if a vertex is an ear
	private void IsVertexEar(Vertex v, List<Vertex> vertices, List<Vertex> earVertices)
	{
		//A reflex vertex cant be an ear!
		if (v.IsReflex)
		{
			return;
		}

		//This triangle to check point in triangle
		Vector2 a = v.PrevVertex.GetPos2D_XZ();
		Vector2 b = v.GetPos2D_XZ();
		Vector2 c = v.NextVertex.GetPos2D_XZ();

		bool hasPointInside = false;

		for (int i = 0; i < vertices.Count; i++)
		{
			//We only need to check if a reflex vertex is inside of the triangle
			if (vertices[i].IsReflex)
			{
				Vector2 p = vertices[i].GetPos2D_XZ();

				//This means inside and not on the hull
				if (IsPointInTriangle(a, b, c, p))
				{
					hasPointInside = true;

					break;
				}
			}
		}

		if (!hasPointInside)
		{
			earVertices.Add(v);
		}
	}

	private bool IsPointInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
	{
		bool isWithinTriangle = false;

		//Based on Barycentric coordinates
		float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

		float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
		float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
		float c = 1 - a - b;

		//The point is within the triangle
		if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
		{
			isWithinTriangle = true;
		}

		return isWithinTriangle;
	}
}




//private GameObject ConstructBuildingTopFace_Concave(BlockGeneration.Block block, List<Vector3> botNodes, float bldgHeight, Material mat)
//{
//	//Step 0. Check if the block stores the vertexes counter clockwise or not
//	//bool counterClockwise = IsCounterClockwise(botNodes);

//	//The list with triangles, that the ear clipping algorithm will generate
//	List<Triangle> triangles = new List<Triangle>();

//	//Step 1. Store the vertices in a list and we also need to know the next and prev vertex
//	List<Vertex> vertices = new List<Vertex>();

//	//if (counterClockwise)
//	{
//		for (int i = 0; i < botNodes.Count; i++)
//			vertices.Add(new Vertex(new Vector3(botNodes[i].x, 0f, botNodes[i].z)));
//	}

//	//else //The method requires the vertex list to be counter-clockwise
//	//{
//	//    for (int i = botNodes.Count - 1; i > -1; i--)
//	//    {
//	//        vertices.Add(new Vertex(new Vector3(botNodes[i].x, 0f, botNodes[i].z)));
//	//    }
//	//}

//	//Find the next and previous vertex
//	vertices[0].PrevVertex = vertices[vertices.Count - 1];
//	vertices[0].NextVertex = vertices[1];

//	vertices[vertices.Count - 1].PrevVertex = vertices[vertices.Count - 2];
//	vertices[vertices.Count - 1].NextVertex = vertices[0];

//	for (int i = 1; i < vertices.Count - 1; i++)
//	{
//		vertices[i].PrevVertex = vertices[i - 1];
//		vertices[i].NextVertex = vertices[i + 1];
//	}

//	//Step 2. Find the reflex (concave) and convex vertices, and ear vertices
//	for (int i = 0; i < vertices.Count; i++)
//	{
//		CheckIfReflexOrConvex(vertices[i]);
//	}

//	//Have to find the ears after we have found if the vertex is reflex or convex
//	List<Vertex> earVertices = new List<Vertex>();

//	for (int i = 0; i < vertices.Count; i++)
//	{
//		IsVertexEar(vertices[i], vertices, earVertices);
//	}

//	//Step 3. Triangulate!
//	while (true)
//	{
//		//This means we have just one triangle left
//		if (vertices.Count == 3)
//		{
//			//The final triangle
//			triangles.Add(new Triangle(vertices[0].Position, vertices[0].PrevVertex.Position, vertices[0].NextVertex.Position));

//			break;
//		}

//		if (earVertices.Count == 0)
//		{
//			Debug.Log("earVertices not found, Triangulation failed.");
//			break;
//		}

//		//Make a triangle of the first ear
//		Vertex earVertex = earVertices[0];

//		Vertex earVertexPrev = earVertex.PrevVertex;
//		Vertex earVertexNext = earVertex.NextVertex;

//		Triangle newTriangle = new Triangle(earVertex.Position, earVertexPrev.Position, earVertexNext.Position);

//		triangles.Add(newTriangle);

//		//Remove the vertex from the lists
//		earVertices.Remove(earVertex);
//		vertices.Remove(earVertex);

//		//Update the previous vertex and next vertex
//		earVertexPrev.NextVertex = earVertexNext;
//		earVertexNext.PrevVertex = earVertexPrev;

//		//...see if we have found a new ear by investigating the two vertices that was part of the ear
//		CheckIfReflexOrConvex(earVertexPrev);
//		CheckIfReflexOrConvex(earVertexNext);

//		earVertices.Remove(earVertexPrev);
//		earVertices.Remove(earVertexNext);

//		IsVertexEar(earVertexPrev, vertices, earVertices);
//		IsVertexEar(earVertexNext, vertices, earVertices);
//	}





//	//Step4 insert the triangles inside the BlockMesh
//	GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
//	gameObject.name = "Topface";
//	gameObject.GetComponent<MeshRenderer>().material.CopyPropertiesFromMaterial(mat);


//	List<Vector3> mesh_vertices = new List<Vector3>();
//	List<Vector2> mesh_uv = new List<Vector2>();
//	List<int> mesh_triangles = new List<int>();
//	int count = 0;

//	float minX, minZ, maxX, maxZ;
//	minX = minZ = +999999.0f;
//	maxX = maxZ = -999999.9f;

//	foreach (var node in botNodes)
//	{
//		minX = Mathf.Min(node.x, minX);
//		minZ = Mathf.Min(node.z, minZ);

//		maxX = Mathf.Max(node.x, maxX);
//		maxZ = Mathf.Max(node.z, maxZ);
//	}

//	foreach (Triangle tri in triangles)
//	{

//		Vector3 v0 = new Vector3(tri.A.x, tri.A.y, tri.A.z) + new Vector3(0, bldgHeight, 0); //We want to draw the blockMeshes as gizmos right now! (up is z right now in the 2d model)
//		Vector3 v1 = new Vector3(tri.B.x, tri.B.y, tri.B.z) + new Vector3(0, bldgHeight, 0);
//		Vector3 v2 = new Vector3(tri.C.x, tri.C.y, tri.C.z) + new Vector3(0, bldgHeight, 0);

//		Vector2 t0 = new Vector2((v0.x - minX) / (maxX - minX), (v0.z - minZ) / (maxZ - minZ));
//		Vector2 t1 = new Vector2((v1.x - minX) / (maxX - minX), (v1.z - minZ) / (maxZ - minZ));
//		Vector2 t2 = new Vector2((v2.x - minX) / (maxX - minX), (v2.z - minZ) / (maxZ - minZ));

//		mesh_vertices.Add(v0); mesh_vertices.Add(v1); mesh_vertices.Add(v2);
//		mesh_uv.Add(t0); mesh_uv.Add(t1); mesh_uv.Add(t2);

//		mesh_triangles.Add(count + 0); mesh_triangles.Add(count + 1); mesh_triangles.Add(count + 2);
//		count += 3;

//	}



//	Mesh mesh = new Mesh();
//	mesh.vertices = mesh_vertices.ToArray();
//	mesh.uv = mesh_uv.ToArray();
//	mesh.triangles = mesh_triangles.ToArray();

//	mesh.RecalculateNormals();
//	mesh.RecalculateTangents();

//	gameObject.GetComponent<MeshFilter>().mesh = mesh;

//	return gameObject;


//}