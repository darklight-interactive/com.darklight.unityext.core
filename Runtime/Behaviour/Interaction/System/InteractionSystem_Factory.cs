using System.Collections.Generic;
using Darklight.Utility;
using UnityEngine;

namespace Darklight.Behaviour
{
    public abstract partial class InteractionSystem<TData, TType>
        where TData : InteractableData
        where TType : System.Enum
    {
        static class Factory
        {
            const string ASSET_PATH = "Assets/Resources/Darklight/InteractionSystem";
            const string SETTINGS_PATH = ASSET_PATH + "/Settings";
            const string SETTINGS_NAME = "InteractionSystemSettings";

            const string REQUEST_PATH = ASSET_PATH + "/InteractionRequest";
            const string REQUEST_BASE_NAME = "InteractionRequest";

            public static InteractionSystemSettings CreateSettings()
            {
                InteractionSystemSettings settings =
                    ScriptableObjectUtility.CreateOrLoadScriptableObject<InteractionSystemSettings>(
                        ASSET_PATH,
                        SETTINGS_NAME
                    );
                return settings;
            }

            static void InstantiateInteractionReciever(
                Interactable<TData, TType> interactable,
                TType key,
                out GameObject gameObject
            )
            {
                GameObject prefab = Instance.RecieverLibrary[key];

                GameObject recieverGameObject = Instantiate(prefab, interactable.transform);
                if (recieverGameObject == null)
                {
                    Debug.LogError(
                        $"CreateInteractionReciever failed for key {key}. GameObject is null.",
                        interactable
                    );
                    gameObject = null;
                }
                else
                {
                    recieverGameObject.transform.localPosition = Vector3.zero;
                    recieverGameObject.transform.localRotation = Quaternion.identity;
                    recieverGameObject.transform.localScale = Vector3.one;
                }

                InteractionReciever<TData, TType> reciever = recieverGameObject.GetComponent<
                    InteractionReciever<TData, TType>
                >();
                if (reciever == null)
                {
                    Debug.LogError(
                        $"CreateInteractionHandler failed for key {key}. GameObject does not contain InteractionReciever.",
                        interactable
                    );
                    ObjectUtility.DestroyAlways(recieverGameObject);
                    gameObject = null;
                }

                interactable.ActiveRecievers[key] = reciever;
                gameObject = recieverGameObject;
            }

            public static void GenerateInteractableRecievers(
                Interactable<TData, TType> interactable
            )
            {
                Debug.Log($"Generating recievers for {interactable.gameObject.name}", interactable);

                IEnumerable<TType> requestedKeys = interactable.RecieverRequest;
                interactable.ActiveRecievers.Reset();

                foreach (TType key in requestedKeys)
                {
                    interactable.ActiveRecievers.TryGetValue(
                        key,
                        out InteractionReciever<TData, TType> interactableReciever
                    );
                    if (interactableReciever == null)
                    {
                        InteractionReciever<TData, TType> recieverInChild = GetRecieverInChildren(
                            interactable,
                            key
                        );
                        if (recieverInChild != null)
                        {
                            interactable.ActiveRecievers[key] = recieverInChild;
                            continue;
                        }

                        InstantiateInteractionReciever(
                            interactable,
                            key,
                            out GameObject recieverGameObject
                        );
                    }
                    else
                    {
                        Debug.Log($"Reciever for {key} already exists", interactable);
                        //currRequestedReciever.transform.localPosition = Vector3.zero;
                    }
                }

                RemoveUnusedRecievers(interactable);
                InitializeRecievers(interactable);

                //Debug.Log($"Preloaded Interaction Handlers for {Name}. Count {_handlerLibrary.Count}", this);
            }

            public static void RemoveUnusedRecievers(Interactable<TData, TType> interactable)
            {
                InteractionReciever<TData, TType>[] allRecieversInChildren =
                    interactable.GetComponentsInChildren<InteractionReciever<TData, TType>>();
                foreach (InteractionReciever<TData, TType> childReciever in allRecieversInChildren)
                {
                    // If the reciever is not in the library, destroy it
                    if (
                        !interactable.ActiveRecievers.ContainsKey(childReciever.InteractionType)
                        || interactable.ActiveRecievers[childReciever.InteractionType]
                            != childReciever
                    )
                    {
                        ObjectUtility.DestroyAlways(childReciever.gameObject);
                    }
                }
            }

            public static InteractionReciever<TData, TType> GetRecieverInChildren(
                Interactable<TData, TType> interactable,
                TType key
            )
            {
                InteractionReciever<TData, TType>[] recievers =
                    interactable.GetComponentsInChildren<InteractionReciever<TData, TType>>();
                foreach (InteractionReciever<TData, TType> reciever in recievers)
                {
                    if (reciever.InteractionType.Equals(key))
                        return reciever;
                }
                return null;
            }

            static void InitializeRecievers(Interactable<TData, TType> interactable)
            {
                List<TType> requestedKeys = interactable.RecieverRequest;
                foreach (TType key in requestedKeys)
                {
                    interactable.ActiveRecievers.TryGetValue(
                        key,
                        out InteractionReciever<TData, TType> reciever
                    );
                    if (reciever != null)
                        reciever.Initialize(interactable);
                }
            }
        }
    }
}
