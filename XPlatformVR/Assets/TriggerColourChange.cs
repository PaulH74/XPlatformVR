using UnityEngine;
using Photon.Pun;

public class TriggerColourChange : MonoBehaviourPun
{
    private Color _OriginColour;
    private Color _RedColour;
    private Material _ObjectMaterial;

    private void Awake()
    {
        _ObjectMaterial = GetComponent<MeshRenderer>().material;
        _OriginColour = _ObjectMaterial.color;
        _RedColour = Color.red;
    }

    [PunRPC]
    private void ChangeColour(bool change)
    {
        if (change)
        {
            _ObjectMaterial.color = _RedColour;
        }
        else
        {
            _ObjectMaterial.color = _OriginColour;
        }
    }

    public void TriggerColour(bool triggerChange)
    {
        photonView.RPC("ChangeColour", RpcTarget.All, triggerChange);
    }

    private void OnTriggerEnter(Collider other)
    {
        PhotonView phView = other.GetComponentInParent<PhotonView>();

        if (!phView.IsMine)
        {
            return;
        }

        if (other.CompareTag("controllerLeft") || (other.CompareTag("controllerRight")))
        {
            TriggerColour(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PhotonView phView = other.GetComponentInParent<PhotonView>();

        if (!phView.IsMine)
        {
            return;
        }

        if (other.CompareTag("controllerLeft") || (other.CompareTag("controllerRight")))
        {
            TriggerColour(false);
        }
    }
}
