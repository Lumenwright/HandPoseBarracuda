using UnityEngine;
using UnityEngine.UI;
using MediaPipe.HandPose;
using System.Collections.Generic;

public sealed class RegionDetection : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] WebcamInput _webcam = null;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] bool _useAsyncReadback = true;
    [Space]
    [SerializeField] Mesh _jointMesh = null;
    [SerializeField] Mesh _boneMesh = null;
    [Space]
    [SerializeField] Material _jointMaterial = null;
    [SerializeField] Material _boneMaterial = null;
    [Space]
    [SerializeField] RawImage _monitorUI = null;

    #endregion

    #region Private members

    HandPipeline _pipeline;

    static List<(float,float)[]> RegionBounds; 

    static readonly int[] FingertipPoints = {3, 4, 7, 8, 11, 12, 15, 16, 19, 20, 13, 17};

    Matrix4x4 CalculateJointXform(Vector3 pos)
      => Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one * 0.07f);

    Matrix4x4 CalculateBoneXform(Vector3 p1, Vector3 p2)
    {
        var length = Vector3.Distance(p1, p2) / 2;
        var radius = 0.03f;

        var center = (p1 + p2) / 2;
        var rotation = Quaternion.FromToRotation(Vector3.up, p2 - p1);
        var scale = new Vector3(radius, length, radius);

        return Matrix4x4.TRS(center, rotation, scale);
    }

    #endregion

    #region MonoBehaviour implementation

    void Start(){
        _pipeline = new HandPipeline(_resources);
        RegionBounds = new List<(float,float)[]>();

    } 
    void OnDestroy()
      => _pipeline.Dispose();

    void LateUpdate()
    {
        // Feed the input image to the Hand pose pipeline.
        _pipeline.UseAsyncReadback = _useAsyncReadback;
        _pipeline.ProcessImage(_webcam.Texture);

        var layer = gameObject.layer;

        // calculate which region majority of fingertips are in
        //int[] counts = new int[RegionBounds.Count];
        int[] counts = new int[2];
        foreach(int pt in FingertipPoints){
            var pos = _pipeline.GetKeyPoint(pt);
            Debug.Log(pos.x);
            if(pos.x > 0f){ // in right hand region
                counts[1]++;
            }
            else {
                counts[0]++;
            }
        }

        if(counts[0] > counts[1]){
            Debug.Log("left");
        }
        else{
            Debug.Log("right");
        }

        // UI update
        _monitorUI.texture = _webcam.Texture;
    }

    #endregion
}
