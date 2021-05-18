using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Mirror;
using UnityEngine;

namespace EventfulLaboratory.Extension
{
    public static class UnityExtension
    {
        public static Vector3 ScaleStatic(this Vector3 a, Vector3 b)
        {
            return new Vector3
            {
                x = a.x * b.x,
                y = a.y * b.y, 
                z = a.z * b.z
            };
        }

        public static Vector3 Copy(this Vector3 vec)
        {
            return new Vector3()
            {
                x = vec.x,
                y = vec.y,
                z = vec.z
            };
        }

        
        public static NetworkIdentity GetNetworkIdentity(this GameObject gameObject) =>
            gameObject.GetComponent<NetworkIdentity>();

        public static void NotifyPlayers(this GameObject gameObject, bool rebuild = true)
        {
            var odm = new ObjectDestroyMessage
            {
                netId = gameObject.GetNetworkIdentity().netId
            };
            
            foreach (var player in Player.List)
            {
                var pc = player.GameObject.GetComponent<NetworkIdentity>()
                    .connectionToClient;
                if (rebuild)
                {
                    pc.Send(odm);
                }

                typeof(NetworkServer).InvokeStaticMethod("SendSpawnMessage", new object[]{gameObject.GetNetworkIdentity(), pc});
            }
        }

        public static bool IsHit(this RaycastHit ray) => ray.collider != null && ray.collider.gameObject != null;

        public static GameObject FindNetworkIdentityParentObj(this GameObject gameObject)
        {
            var middleMan = gameObject;
            while (true)
            {
                var ni = middleMan.GetComponent<NetworkIdentity>();
                if (ni != null && ni.isActiveAndEnabled)
                    return middleMan;
                else
                {
                    if (middleMan.transform.parent == null)
                    {
                        return null;
                    }
                    else
                    {
                        middleMan = middleMan.transform.parent.gameObject;
                    }
                }
            }
        }

        public static Vector3 ClampToOne(this Vector3 vec, float scale = 10f)
        {
            return new Vector3()
            {
                x = vec.x > scale ? 1 : vec.x < -scale ? -1 : 0,
                y = vec.y > scale ? 1 : vec.y < -scale ? -1 : 0,
                z = vec.z > scale ? 1 : vec.z < -scale ? -1 : 0
            };
        }

        public static bool AnyNonZero(this Vector3 vec)
        {
            for (var i = 0; i < 3; i++)
            {
                if (vec[i] != 0) return true;
            }

            return false;
        }

        public static string BuilderSerialize(this GameObject go)
        {
            return $"{go.name}|{go.transform.position}|{go.transform.rotation}|{go.transform.localScale}";
        }

        public static Vector3 ParseVec3(this string str)
        {
            var floats = str.Substring(1, str.Length-2).Split(',').Select(float.Parse).ToList();
            return new Vector3(floats[0], floats[1], floats[2]);
        }


        public static Quaternion ParseQuat(this string str)
        {
            var floats = str.Substring(1, str.Length-2).Split(',').Select(float.Parse).ToList();
            return new Quaternion(floats[0], floats[1], floats[2], floats[3]);
        }

        public static Vector3 CenterPoint(this List<GameObject> list)
        {
            float x = 0, y = 0, z = 0;
            foreach (var position in list.Select(gameObject => gameObject.transform.position))
            {
                x += position.x;
                y += position.y;
                z += position.z;
            }

            return new Vector3(x / list.Count, y / list.Count, z / list.Count);
        }
    }
}