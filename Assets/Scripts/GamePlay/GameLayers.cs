using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjectsLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask grassLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask fovLayer;
    [SerializeField] LayerMask portalLayer;
    [SerializeField] LayerMask triggersLayer;
    [SerializeField] LayerMask ledgeLayer;
    [SerializeField] LayerMask waterLayer;
    [SerializeField] LayerMask sandLayer;
    [SerializeField] LayerMask stairsLayer;

    public static GameLayers i { get; set; }

    private void Awake() => i = this;

    public LayerMask SolidLayer => solidObjectsLayer;
    public LayerMask InteractableLayer => interactableLayer;
    public LayerMask GrassLayer => grassLayer;
    public LayerMask PlayerLayer => playerLayer;
    public LayerMask FovLayer => fovLayer;
    public LayerMask PortalLayer => portalLayer;
    public LayerMask LedgeLayer => ledgeLayer;
    public LayerMask WaterLayer => waterLayer;
    public LayerMask SandLayer => sandLayer;
    public LayerMask StairsLayer => stairsLayer;

    public LayerMask TriggerableLayers => grassLayer | fovLayer | portalLayer | triggersLayer | waterLayer | sandLayer | stairsLayer;
}
