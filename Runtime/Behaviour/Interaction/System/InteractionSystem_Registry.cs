using System;
using Darklight.Collections;
using UnityEngine;

namespace Darklight.Behaviour
{
    public abstract partial class InteractionSystem<TData, TType>
        where TData : InteractableData
        where TType : System.Enum
    {
        [Serializable]
        public class InteractionSystem_Registry
            : CollectionDictionary<int, Interactable<TData, TType>>
        {
            /// <summary>
            /// Attempt to register an interactable. <br/>
            /// 1. If the interactable is not in the library, add it. <br/>
            /// 2. If the interactable is in the library and the same reference, return true. <br/>
            /// 3. If the interactable is in the library and not the same reference, overwrite if allowed. <br/>
            /// </summary>
            /// <param name="interactable"></param>
            /// <param name="overwrite"></param>
            /// <returns></returns>
            public void TryRegisterInteractable(
                Interactable<TData, TType> interactable,
                out bool result,
                bool overwrite = false
            )
            {
                result = false;
                if (ContainsKey(interactable.Data.ID))
                {
                    // If the interactable is in the library and the same reference, return true
                    if (this[interactable.Data.ID] == interactable)
                    {
                        Debug.Log(
                            $"{Prefix} Interactable {interactable.Data.ID} already registered",
                            interactable
                        );
                        result = true;
                    }
                    else if (this[interactable.Data.ID] == null)
                    {
                        Debug.LogWarning(
                            $"{Prefix} Overwriting null value of Interactable {interactable.Print()}",
                            interactable
                        );
                        this[interactable.Data.ID] = interactable;
                        result = true;
                    }
                    else if (this[interactable.Data.ID] != null)
                    {
                        if (overwrite)
                        {
                            Debug.Log(
                                $"{Prefix} Overwriting non-null value of Interactable {interactable.Print()}",
                                interactable
                            );
                            this[interactable.Data.ID] = interactable;
                            result = true;
                        }
                        else
                        {
                            Debug.LogError(
                                $"{Prefix} Interactable {interactable.Print()} already registered",
                                interactable
                            );
                            result = false;
                        }
                    }
                }
                else
                {
                    Add(interactable.Data.ID, interactable);
                    Debug.Log(
                        $"{Prefix} Adding {interactable.Print()} to the Registry",
                        interactable
                    );
                    result = true;
                }

                if (result)
                {
                    Factory.GenerateInteractableRecievers(interactable);
                }
            }

            public bool IsRegistered(Interactable interactable)
            {
                return ContainsKey(interactable.Data.ID);
            }

            public void TryGetInteractable<T>(out T interactable)
                where T : Interactable<TData, TType>
            {
                interactable = null;
                foreach (T item in Values)
                {
                    if (item is T)
                    {
                        interactable = (T)item;
                        break;
                    }
                }
            }
        }
    }
}
