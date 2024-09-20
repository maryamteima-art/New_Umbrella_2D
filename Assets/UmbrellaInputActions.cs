//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/UmbrellaControls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @UmbrellaInputActions: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @UmbrellaInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""UmbrellaControls"",
    ""maps"": [
        {
            ""name"": ""Umbrella"",
            ""id"": ""3a6d87ab-d3f1-422a-b136-8d914c8d2f26"",
            ""actions"": [
                {
                    ""name"": ""Orient"",
                    ""type"": ""Value"",
                    ""id"": ""e7819e04-12b6-43a6-8165-c0f8e390ba5a"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""88dbec96-4498-43e5-946d-88bdefbf8dcf"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Orient"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Umbrella
        m_Umbrella = asset.FindActionMap("Umbrella", throwIfNotFound: true);
        m_Umbrella_Orient = m_Umbrella.FindAction("Orient", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Umbrella
    private readonly InputActionMap m_Umbrella;
    private List<IUmbrellaActions> m_UmbrellaActionsCallbackInterfaces = new List<IUmbrellaActions>();
    private readonly InputAction m_Umbrella_Orient;
    public struct UmbrellaActions
    {
        private @UmbrellaInputActions m_Wrapper;
        public UmbrellaActions(@UmbrellaInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Orient => m_Wrapper.m_Umbrella_Orient;
        public InputActionMap Get() { return m_Wrapper.m_Umbrella; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(UmbrellaActions set) { return set.Get(); }
        public void AddCallbacks(IUmbrellaActions instance)
        {
            if (instance == null || m_Wrapper.m_UmbrellaActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_UmbrellaActionsCallbackInterfaces.Add(instance);
            @Orient.started += instance.OnOrient;
            @Orient.performed += instance.OnOrient;
            @Orient.canceled += instance.OnOrient;
        }

        private void UnregisterCallbacks(IUmbrellaActions instance)
        {
            @Orient.started -= instance.OnOrient;
            @Orient.performed -= instance.OnOrient;
            @Orient.canceled -= instance.OnOrient;
        }

        public void RemoveCallbacks(IUmbrellaActions instance)
        {
            if (m_Wrapper.m_UmbrellaActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IUmbrellaActions instance)
        {
            foreach (var item in m_Wrapper.m_UmbrellaActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_UmbrellaActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public UmbrellaActions @Umbrella => new UmbrellaActions(this);
    public interface IUmbrellaActions
    {
        void OnOrient(InputAction.CallbackContext context);
    }
}
