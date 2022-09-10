using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer),typeof(Animation))]
public class HexGridComponent : MonoBehaviour
{
    public int zoomLevel = -1;
    public Camera cam;
    public Animation _animation;
    public float boundsMultiplier = 1.2f;



    private SpriteRenderer _sprite;
    private State _state;


    private const float DownscaleFactor = 2.645751311064593f;
    private const float CcwRotationAmt = 19.106605350869092f;
    private const float PortionScreen = 0.8f;
    private const float SubdivisionThreshold = 0.35f;
    private const float MaxZoom = 10;


    private static readonly float Root3 = Mathf.Sqrt(3);
    private static readonly int[,] Tiling = {{0, 0}, {0, 1}, {0, -1}, {1, 0}, {-1, 0}, {1, -1}, {-1, 1}};

    // Start is called before the first frame update

    void Start()
    {
        _animation = GetComponent<Animation>();
        _animation.Play("HexFadeIn");
        if (zoomLevel != 0) return;
        _sprite = GetComponent<SpriteRenderer>();
        _state = State.Hidden;
        CreateTopLevel();
    }

    private void CreateTopLevel()
    {
        var height = cam.orthographicSize * 2.0f;

        var scaleFactor = PortionScreen * height / (_sprite.sprite.textureRect.height / _sprite.sprite.pixelsPerUnit);

        transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
        transform.rotation = Quaternion.Euler(0,0,2*CcwRotationAmt);
        transform.position = Vector3.zero;
        _state = State.Loaded;
    }

    private void CreateChildren()
    {
        Transform centerSibling = null;
        for (var i = 0; i < Tiling.GetLength(0); i++)
        {
            var t = Instantiate(Resources.Load("Prefabs/HexGrid") as GameObject, transform, false);
            centerSibling ??= t.transform;
            t.GetComponent<HexGridComponent>()
                .CreateAsChild(new Vector2(Tiling[i, 0], Tiling[i, 1]), centerSibling, this);
        }
    }

    void CreateAsChild(Vector2 axialInParent, Transform centerSibling, HexGridComponent parent)
    {
        zoomLevel = parent.zoomLevel + 1;
        _sprite = GetComponent<SpriteRenderer>();
        cam = parent.cam;
        transform.Rotate(0, 0, -CcwRotationAmt);
        transform.localScale = new Vector3(1 / DownscaleFactor, 1 / DownscaleFactor, 1);
        var newTransform = Root3 * AxialToCartesian(axialInParent) / MathF.Pow(DownscaleFactor, zoomLevel - 1);
        transform.position = Vector3.back + centerSibling.position + centerSibling.TransformDirection(newTransform);

        _sprite.color = parent._sprite.color.RandomOffset(0.05f);
    }

    private Vector3 AxialToCartesian(Vector3 axialCoords)
    {
        var x = 3.0f / 2 * axialCoords.x;
        var y = Root3 / 2 * axialCoords.x + Root3 * axialCoords.y;
        return new Vector2(x, y);
    }


    // Update is called once per frame
    void Update()
    {
        switch (_state)
        {
            case State.Hidden:
                HiddenAction();
                break;
            case State.Loaded:
                LoadedAction();
                break;
            case State.Subdivided:
                SubdividedAction();
                break;
            case State.ShouldDestroy:
                if (!_animation.isPlaying)
                {
                    Destroy(this.gameObject);
                }
                break;;
        }
    }

    void HiddenAction()
    {
        if (InView())
        {
            _animation.Stop();
            _animation.Play("HexFadeIn");
            _state = State.Loaded;
        }
    }

    void LoadedAction()
    {
        if (!InView())
        {
            _animation.Stop();
            _sprite.enabled = false;
            _state = State.Hidden;
        }
        else if (GetHeightInCamera() > SubdivisionThreshold && zoomLevel < MaxZoom)
        {
            CreateChildren();
            _animation.Stop();
            _animation.Play("HexFadeOut");
            _state = State.Subdivided;
        }
    }

    private void SubdividedAction()
    {
        if (!InView())
        {
            _sprite.enabled = false;
            DestroyChildren();
            _state = State.Hidden;
        }
        else if (GetHeightInCamera() is <= SubdivisionThreshold and > -2.0f)
        {
            _animation.Play("HexFadeIn");
            DestroyChildrenWithAnim();
            _state = State.Loaded;
        }
    }

    private void DestroyChildrenWithAnim()
    {
        foreach (Transform child in transform)
        {
            var childGridComponent = child.GetComponent<HexGridComponent>();
            childGridComponent._animation.Play("HexFadeOut");
            childGridComponent._state = State.ShouldDestroy;
        }
    }

    private void DestroyChildren()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private bool InView()
    {
        var planes = GeometryUtility.CalculateFrustumPlanes(cam);
        Bounds bounds = _sprite.bounds;
        bounds.Expand(bounds.extents.magnitude * (boundsMultiplier-1));
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }
    
    private float GetHeightInCamera()
    {
        if (!_sprite)
            return -1.0f;

        var planes = GeometryUtility.CalculateFrustumPlanes(cam);
        if (!GeometryUtility.TestPlanesAABB(planes, _sprite.bounds)) // check if it's off screen
            return -2.0f;

        var cMin = cam.WorldToScreenPoint(_sprite.bounds.min);
        var cMax = cam.WorldToScreenPoint(_sprite.bounds.max);

        cMin.x -= cam.pixelRect.x; // adjust for camera position on screen
        cMin.y -= cam.pixelRect.y;
        cMax.x -= cam.pixelRect.x;
        cMax.y -= cam.pixelRect.y;

        return Mathf.Abs((cMax.y - cMin.y) / cam.pixelHeight);
    }

    private enum State
    {
        Loaded,
        Subdivided,
        Hidden,
        ShouldDestroy
    }
}