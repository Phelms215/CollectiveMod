using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Collective.Components.DataSets;
using Collective.Components.Definitions;
using Collective.Components.Modals;
using Collective.Definitions;
using Collective.Systems.Managers;
using DG.Tweening;
using Lean.Pool;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace Collective.Systems.Entities;

public class StaffMember : MonoBehaviour
{
    public Guid Guid;
    private readonly Vector3 _startPosition = new Vector3(4.868f, 0, 4.8341f);
    private Employee _employee;
    private Cashier _cashier;
    private Customer _customer;
    private NavMeshAgent _navMeshAgent;
    private int _employeeId = 0;
    private Transform _camera;
    private GameObject _nameTagObject;
    private TextMeshPro _nameTag; // Added a field to hold the reference to the TextMeshPro

    private bool _atTask = false;
    private bool _doneMoving = false;
    private bool _shiftOver = false;
    private Checkout _checkOut;



    private Hours? _taskTimeOut = null;
    private RestockerTask? _restockerTask;
    private int _productID = 0;
    private Box m_Box; 
    private int m_CurrentBoxLayer = 0;
    public Transform m_BoxHolder;



    public void Setup(Employee employee, int employeeId)
    {
        _employeeId = employeeId;
        _employee = employee;
        _camera = GameObject.Find("Player/Camera Pivot").transform;

        var customerPrefabs =
            CustomerGenerator.Instance.m_CustomerPrefabs.FirstOrDefault(x => x.name == _employee.PrefabName);
        var spawningTransform = CustomerGenerator.Instance.SpawningTransforms.GetRandom();
        var additionalRotation = Quaternion.Euler(0, 180, 0);
        if (spawningTransform == null) throw new Exception("No spawning transform found");
        var newRotation = spawningTransform.rotation * additionalRotation;
        _customer = LeanPool.Spawn<Customer>(customerPrefabs, spawningTransform.position, newRotation);
        _navMeshAgent = _customer.gameObject.GetComponent<NavMeshAgent>();
 
        _nameTagObject = new GameObject("Collective-StaffNameTag" + employee.Guid);
        _nameTag = _nameTagObject.AddComponent<TextMeshPro>();
        _nameTag.text = _employee.Name;
        _nameTag.alignment = TextAlignmentOptions.Center;
        _nameTag.fontSize = 3; // Adjust size as necessary
        _nameTagObject.transform.SetParent(_customer.transform);
        _nameTagObject.transform.localPosition =
            new Vector3(0, 1.7f, 0); // Offset to position the text above the customer
        _nameTagObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f); // Adjust scale as necessary


        CreateEmployee();
    }


    public void CompleteAnimation(Cashier cashier, int animation)
    {
        if (cashier != _cashier) return;
        _customer.m_Animator.m_Animator.SetTrigger(animation);
    }


    public void GoToWork()
    {
        _employee.IsCurrentlyWorking = true;
        _customer.StartCoroutine(Travel(_startPosition, () => true));
    }

    public void ReloadTask()
    { 
        _customer.transform.position = _startPosition;
        _atTask = false;
        _doneMoving = true;
        
    }

    public void ShouldGoHome()
    {

        // Determine if they should go home
        var currentTime = Collective.GetNormalizedTime();
        if (_employee.NextShift == null) return;
        if (!_shiftOver &&
            _employee.NextShift.Close.Hour <= currentTime.Hour &&
            _employee.NextShift.Close.Minute <= currentTime.Minute)
        {
            // Mark shift over
            _shiftOver = true;
        }

    }

    private void CreateEmployee()
    {
        if (_employee.JobRole == JobRole.Cashier)
        {

            _cashier = Singleton<EmployeeGenerator>.Instance.SpawnCashier(Singleton<IDManager>.Instance
                .CashierSO(_employeeId).CashierPrefab);
            _cashier.CashierID = _employeeId;
            _cashier.transform.GetChild(0).gameObject.SetActive(false);
            return;
        }

    }

    public Vector3 GetStaffMemberPosition() => _customer.transform.position;

    private void LateUpdate()
    {
        var position = _nameTagObject.transform.position;
        Vector3 directionToCamera = _camera.transform.position - position;
        directionToCamera.y = 0; // Optionally keep the billboard aligned strictly horizontally

        // Make the nametag face the camera
        _nameTagObject.transform.LookAt(position + directionToCamera);

        // Adjust the rotation to prevent the text from being mirrored
        _nameTagObject.transform.Rotate(0, 180f, 0);
    }

    private StoreInventory GetStoreInventory() => Collective.GetManager<DistributionManager>().GetStoreInventory();

    private void Update()
    {
        if (!_doneMoving)
        {
            AdjustEmployeeRotation();
            return;
        }

        switch (_employee.JobRole)
        {
            case JobRole.Cashier:
                CashierUpdate();
                return;
            case JobRole.Stocker:
                StockerUpdate();
                return;
        }
    }

    private void CashierUpdate()
    {
        if (_shiftOver)
        {
            if (_atTask)
            {
                if (_checkOut?.m_Customers.Count > 0) return;
                _checkOut.RemoveCashier();
                _checkOut.ClearCheckout();
            }

            _customer.StartCoroutine(
                Travel(CustomerGenerator.Instance.SpawningTransforms.GetRandom().transform.position, EndShift));
            return;
        }

        if (!_atTask)
            GoToRegister();
    }


    private void StockerUpdate()
    {
        if (_shiftOver)
        {
            if (_restockerTask != null) return;
            
            // Drop the box off before leaving.
            if (m_Box != null && m_Box.HasProducts)
            { 
                ReturnProductToStorage();
                return;
            }
            
            // If the box 
            if (m_Box != null && !m_Box.HasProducts)
                DeleteBox(); 
            
            // Go home.
            _customer.StartCoroutine(Travel(CustomerGenerator.Instance.SpawningTransforms.GetRandom().transform.position, EndShift));
            return;
        }

        if (_atTask) return; 
        if (m_Box != null && !m_Box.HasProducts)
        {
            DeleteBox(); 
            return;
        } 
        
        // Only get a new task if we don't have one
        if(_restockerTask != null) return;
       
            
        
        // Fetch new task with product in hand
        _restockerTask = Collective.GetManager<StaffManager>().FetchNewTask(_employee, _productID);
        
        // We can't find a task with tis product, go put it back in storage 
        if (_restockerTask == null && _productID != 0) ReturnProductToStorage();
        
        // No tasks to do 
        if (_restockerTask == null && _customer.transform.position != _startPosition)
        {
            _customer.StartCoroutine(Travel(_startPosition, () => true));
            return;
        }
        if(_restockerTask == null) return;

        // We got a task!
        Collective.Log.Info($"Employee: {_employee.Name} is restocking product: {_restockerTask.ProductId} with task id {_restockerTask.ID}");
        RestockShelfTask();
    }

    private void ReturnProductToStorage()
    {
        if (m_Box == null) return;
        var productId = m_Box.Product.ID;
        var rackWithSpace = GetStoreInventory().GetRackSlotsWithSpace(productId).FirstOrDefault();
        if (rackWithSpace == null)
            rackWithSpace = GetStoreInventory().GetEmptyRackSlot();
            
        _customer.StartCoroutine(Travel(rackWithSpace.transform.position, () =>
        {
            _customer.transform.rotation = rackWithSpace.InteractionRotation;
            PlaceBox(rackWithSpace);
            _restockerTask = null;
            m_Box = null;
            _productID = 0;
            return true;
        }));
    }

    private void RestockShelfTask()
    {
        if (_restockerTask == null) return;
        if (_restockerTask.TaskType != RestockerTaskTypes.RestockShelf) return;
        
        _atTask = true;
        // WE are already holdign the product in our hands 
        if (m_Box != null && _productID != 0 && _productID == _restockerTask.ProductId)
        {
            _customer.StartCoroutine(Travel(_restockerTask.TargetDisplaySlot.InteractionPosition,  EmployeeAtTargetDisplaySlot)); 
            return;
        }
        
        
        // Find inventory and then go fill shelf
        var rack = GetStoreInventory().FindItemInventory(_restockerTask.ProductId);
        if (rack.Count == 0)
        {
            MarkTaskDone();
            return;
        }

        _restockerTask.TargetRackSlot = rack.First().RackSlot;
        _customer.StartCoroutine(Travel(_restockerTask.TargetRackSlot.InteractionPosition, EmployeeAtTargetRackSlot));

    }

    private bool EmployeeAtTargetRackSlot()
    {
        if (_restockerTask == null) return true;
        
        // Pick up product
        PickUpBox(_restockerTask.TargetRackSlot); 
        _customer.StartCoroutine(Travel(_restockerTask.TargetDisplaySlot.InteractionPosition,  EmployeeAtTargetDisplaySlot));  
        return true;  
    }
 

    private bool EmployeeAtTargetDisplaySlot()
    {
        if (_restockerTask == null) return true;
 
        // Place Product
        _customer.StartCoroutine(PlaceProducts());
        return true;
    }
 

    private bool EndShift()
    {
        _employee.IsCurrentlyWorking = false;
        _employee.SetGamePosition(Vector3.zero);
        Collective.GetManager<StaffManager>().RemoveStaff(_employee.Guid);
        Destroy(_customer);
        if(_employee.JobRole == JobRole.Cashier)
            Destroy(_cashier.gameObject);
        
        Destroy(gameObject);
        return true;
    }

    private IEnumerator Travel(Vector3 destination, Func<bool> action)
    { 
        _doneMoving = false;

        // Ensure the agent is active
        _navMeshAgent.enabled = true;
        _navMeshAgent.SetDestination(destination);
         
        // Wait for the path to become valid
        yield return new WaitWhile(() => _navMeshAgent.pathPending); 
        
        // Check if the agent has a path and it's not already complete
        if (_navMeshAgent.enabled && _navMeshAgent.hasPath && _navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance)
        {
            // Wait until the agent reaches the destination
            yield return new WaitUntil(() => _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance);
        }
        else
        {
            _navMeshAgent.enabled = true;
            // If the remaining distance is already within the stopping distance
            // We force a small wait to ensure it's not a false positive from a very close new destination
            yield return new WaitForSeconds(1f);
            if (_navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance)
            {
                yield return new WaitUntil(() => _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance);
            }
        }

        // Stop the agent if no longer needed to move
        _doneMoving = true;
        _navMeshAgent.enabled = false;
 
        action.Invoke(); 
        
    }


    private void GoToRegister()
    {
        GetCheckout();
        if (_checkOut.Data == null) return;
        _customer.StartCoroutine(Travel(_checkOut.Data.Transform.Position, () =>
        {
            _checkOut.AddCashier(_cashier);
            var transform1 = _cashier.transform;
            var transform2 = _customer.transform;
            transform2.position = transform1.position;
            transform2.rotation = transform1.rotation;
            _atTask = true;
            return true;
        }));

    }

    private void GetCheckout() => _checkOut = Singleton<CheckoutManager>.Instance.GetAvailableCheckoutForEmployee();
 
    private void AdjustEmployeeRotation()
    {
        // Ensure that all necessary objects are instantiated before using them
        if (_navMeshAgent == null || _customer == null || _customer.transform == null) return;

        // Check if the agent is moving
        if (_navMeshAgent.velocity.magnitude > 0.1f) 
        {
            var lookRotation = Quaternion.LookRotation(_navMeshAgent.velocity.normalized);
            _customer.transform.rotation = Quaternion.Slerp(_customer.transform.rotation, lookRotation, Time.deltaTime * 5);
        }
    }


    //
    // Box System
    // 

    public void DeleteBox()
    {
        Collective.Log.Info($"DELETE THIS BOX");
        _productID = 0;
        LeanPool.Despawn(this.m_Box.gameObject);
        Singleton<InventoryManager>.Instance.RemoveBox(this.m_Box.Data);
        this.m_Box = (Box)null;
    }
    
    

    private void PickUpBox(RackSlot targetRackSlot)
    {
        Box boxFromRack = targetRackSlot.TakeBoxFromRack();
        if (boxFromRack == null)
        {
            Collective.Log.Info("There's no box coming from target rack slot: " + targetRackSlot.gameObject.name); 
            return;
        } 
        
        foreach (Collider componentsInChild in boxFromRack.GetComponentsInChildren<Collider>())
            componentsInChild.isTrigger = true;
        
        // Parent the box to the customer
        boxFromRack.transform.SetParent(_customer.transform);

        // Set the local position of the box to appear in front and at waist height of the character
        // Assuming the character's forward direction and a reasonable height offset
        Vector3 newPosition = new Vector3(0, 1.0f, 0.5f);  // Adjust these values to better fit your character's model
        boxFromRack.transform.DOLocalMove(newPosition, 0.3f);  // Smooth transition to the new position
    
        // Set the local rotation of the box to match the forward direction of the character
        Quaternion newRotation = Quaternion.Euler(0, 0, 0);  // Adjust if necessary to align the box correctly
        boxFromRack.transform.DOLocalRotateQuaternion(newRotation, 0.3f);  // Smooth rotation transition

        // Update box state and product ID
        this.m_Box = boxFromRack;
        this.m_Box.Racked = false;
        this.m_CurrentBoxLayer = (LayerMask) this.m_Box.gameObject.layer;
        this.m_Box.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        _productID = this.m_Box.Product.ID;
    }

    private IEnumerator PlaceProducts()
    {
 
        if (m_Box == null || _restockerTask == null || _restockerTask.TargetDisplaySlot == null)
        {
            Collective.Log.Info("PlaceProducts called with null conditions.");
            yield break; // Safely exit the coroutine if conditions are not met.
        }
 
        var targetDisplaySlot = _restockerTask.TargetDisplaySlot;


        if (!this.m_Box.IsOpen)
        {
            this.m_Box.OpenBox();
            yield return (object)new WaitForSeconds(0.3f);
        }
 
        while (m_Box != null && !targetDisplaySlot.Full && this.m_Box.HasProducts)
        {
            Product productFromBox = this.m_Box.GetProductFromBox(); 
            targetDisplaySlot.AddProduct(_productID, productFromBox);
            Singleton<InventoryManager>.Instance.AddProductToDisplay(new ItemQuantity()
            {
                Products = new Dictionary<int, int>()
                {
                    {
                        _productID,
                        1
                    }
                }
            });
            yield return (object)new WaitForSeconds(0.1f);
        }
 
        MarkTaskDone();
    }

    private void MarkTaskDone()
    {
        Collective.GetManager<StaffManager>().CompleteTask(_restockerTask.ID);
        _restockerTask = null;
        _atTask = false; 
    }

    private void PlaceBox(RackSlot rackSlot)
    {
        if (this.m_Box == null)
            return;
        this.m_Box.gameObject.layer = (int) this.m_CurrentBoxLayer;
        foreach (Collider componentsInChild in this.m_Box.GetComponentsInChildren<Collider>())
            componentsInChild.isTrigger = false;
        rackSlot.AddBox(this.m_Box.BoxID, this.m_Box);
        this.m_Box.Racked = true;
        this.m_Box = (Box) null;  
        _productID = 0;
    }

}

