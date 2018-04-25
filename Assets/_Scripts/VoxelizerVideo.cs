using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VoxelizerVideo : MonoBehaviour {

    [SerializeField] VideoPlayer _sourceVideo;

    [Space]
    [SerializeField] int _columns = 32;
    [SerializeField] int _rows = 18;
    [SerializeField] Mesh _baseMesh;

    [SerializeField /*, HideInInspector*/] Shader _feedbackShader;
    [SerializeField] Material _voxelMaterial;

    RenderTexture _feedbackBuffer;
    Material _feedbackMaterial;
    MaterialPropertyBlock _props;
    Mesh _bulkMesh;

    [SerializeField, Range(0, 1)] float _threshold = 0.05f;
    [SerializeField, Range(1, 10)] float _decaySpeed = 5;

    [SerializeField] float _voxelScale = 0.25f;

    [SerializeField] float _zMove = -0.1f;
    [SerializeField] float _noiseFrequency = 10;
    [SerializeField] float _noiseSpeed = 0.5f;
    [SerializeField] float _noiseToPosition = 0.015f;
    [SerializeField] float _noiseToRotation = 60;
    [SerializeField] float _noiseToScale = 0.5f;

    [SerializeField] Vector2 _extent = new Vector2(3.2f, 1.8f);

    //[SerializeField] Renderer _renderer;

    //[SerializeField] MeshRenderer _rend;

	void Start () 
    {
        _bulkMesh = BuildBulkMesh();
        _feedbackMaterial = new Material(_feedbackShader);
        _props = new MaterialPropertyBlock();	
        //_rend.GetComponent<MeshFilter>().mesh = _bulkMesh;
	}

    void Update()
    {
        _feedbackMaterial = new Material(_feedbackShader);
        var rt = RenderTexture.GetTemporary(
            _columns, _rows, 0,  RenderTextureFormat.RHalf
        );

        _feedbackMaterial.SetTexture("_PrevTex", _feedbackBuffer);
        _feedbackMaterial.SetFloat("_Convergence", -_decaySpeed);

        Graphics.Blit(_sourceVideo.texture, rt, _feedbackMaterial, 0);

        if (_feedbackBuffer != null)
            RenderTexture.ReleaseTemporary(_feedbackBuffer);
        _feedbackBuffer = rt;

        _props.SetTexture("_ModTex", _feedbackBuffer);
        _props.SetFloat("_Threshold", _threshold);
        _props.SetVector("_Extent", _extent);
        _props.SetFloat("_ZMove", Random.Range(_zMove, 0f));
        _props.SetFloat("_Scale", _voxelScale);
        _props.SetVector("_NoiseParams", new Vector2(
            _noiseFrequency, _noiseSpeed
        ));
        _props.SetVector("_NoiseAmp", new Vector3(
            _noiseToPosition, Mathf.Deg2Rad * _noiseToRotation, _noiseToScale
        ));

        //_renderer.material.mainTexture = _feedbackBuffer;

        /*Graphics.DrawMesh(_baseMesh,
                          transform.localToWorldMatrix,
                          _voxelMaterial,
                          gameObject.layer,
                          null,
                          0,
                          _props
                         );*/


       // _rend.SetPropertyBlock(_props);

        Graphics.DrawMesh(
                _bulkMesh, transform.localToWorldMatrix, _voxelMaterial,
                gameObject.layer, null, 0, _props
            );
    }

    #region Bulk mesh construction

    Mesh BuildBulkMesh()
    {
        var instanceCount = _columns * _rows;

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

        for (var row = 0; row < _rows; row++)
        {
            for (var col = 0; col < _columns; col++)
            {
                var uv = new Vector2(
                    (col + 0.5f) / _columns,
                    (row + 0.5f) / _rows
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

    #endregion
}
