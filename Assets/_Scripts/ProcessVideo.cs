using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class ProcessVideo : MonoBehaviour {

    [SerializeField] VideoPlayer _sourceVideo;
    [SerializeField] Shader _computeShader;
    [SerializeField] Renderer _renderer;
    [SerializeField] Mesh _baseMesh;
    [SerializeField] Material _outMaterial;

    static int _res = 15;
    Material _computeMaterial;
    MaterialPropertyBlock _props;
    RenderTexture _computeBuffer;
    Mesh _bulkMesh;

	void Start () 
    {
        _computeMaterial = new Material(_computeShader);
        _props = new MaterialPropertyBlock();
        _bulkMesh = BuildBulkMesh();
	}
	
	void Update () 
    {
        _computeMaterial = new Material(_computeShader);
        var rt = RenderTexture.GetTemporary(
            _res, _res, 0, RenderTextureFormat.RHalf
        );

        Graphics.Blit(_sourceVideo.texture, rt, _computeMaterial, 0);

       // _computeMaterial.SetTexture("_PrevTex", _computeBuffer);
 
        if (_computeBuffer != null)
            RenderTexture.ReleaseTemporary(_computeBuffer);
        _computeBuffer = rt;

        //_renderer.material.mainTexture = _computeBuffer;

        _props.SetTexture("_ModTex", _computeBuffer);

        Graphics.DrawMesh(
                _bulkMesh, transform.localToWorldMatrix, _outMaterial,
                gameObject.layer, null, 0, _props
            );
	}



    Mesh BuildBulkMesh()
    {
        var instanceCount = _res * _res;

        var iVertices = _baseMesh.vertices;
        var iNormals = _baseMesh.normals;
        var iUVs = _baseMesh.uv;

        var oVertices = new List<Vector3>(iVertices.Length * instanceCount);
        var oNormals = new List<Vector3>(iNormals.Length * instanceCount);
        var oUVs = new List<Vector2>(iUVs.Length * instanceCount);

        for (var i = 0; i < instanceCount; i++)
        {
            oVertices.AddRange(iVertices);
            oNormals.AddRange(iNormals);
            oUVs.AddRange(iUVs);
        }

        var oUV2 = new List<Vector2>(oUVs.Count);

        for (var row = 0; row < _res; row++)
        {
            for (var col = 0; col < _res; col++)
            {
                var uv = new Vector2(
                    (col + 0.5f) / _res,
                    (row + 0.5f) / _res
                );

                for (var i = 0; i < _baseMesh.vertexCount; i++)
                    oUV2.Add(uv);
            }
        }

        var iIndices = _baseMesh.triangles;
        var oIndices = new List<int>(iIndices.Length * instanceCount);

        for (var i = 0; i < instanceCount; i++)
        {
            for (var j = 0; j < iIndices.Length; j++)
            {
                oIndices.Add(iIndices[j]);
                iIndices[j] += _baseMesh.vertexCount;
            }
        }

        var mesh = new Mesh();
        mesh.SetVertices(oVertices);
        mesh.SetNormals(oNormals);
        mesh.SetUVs(0, oUVs);
        mesh.SetUVs(1, oUV2);
        mesh.subMeshCount = 2;
        mesh.SetTriangles(oIndices, 0);
        mesh.UploadMeshData(true);
        return mesh;
    }

}
