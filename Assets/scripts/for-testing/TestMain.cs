using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMain : MonoBehaviour 
{
  public GameObject TestObjectPrefab;

  TestInstantiated _reference;
 
  GameObject _testObjectInstance;

  TestReferenceClass _testRefClass = new TestReferenceClass();
  TestClass _testClassRef;

  void Start()
  {    
    _testObjectInstance = (GameObject)Instantiate(TestObjectPrefab);

    _reference = _testObjectInstance.GetComponent<TestInstantiated>();

    _testRefClass.TestObjectReference = _reference;

    // Reference to TestClass inside MonoBehaviour is still present even after MonoBehaviour is destroyed
    _testClassRef = _reference.TestClassRef;
  }

  float _timer = 0.0f;
  void Update()
  {
    _timer += Time.smoothDeltaTime;

    if (_timer > 1.0f)
    {      
      Debug.Log("_testRefClass.TestObjectReference: " + _testRefClass.TestObjectReference + "\n_reference: " + _reference + "\n_testObjectInstance: " + _testObjectInstance + "\nTestClassReference: " + _testClassRef);
      _timer = 0.0f;
    }
  }
}

public class TestClass
{
  int _value = 0;
  public Int2 Object = new Int2();

  public TestClass()
  {    
    _value = -1;
    Object.Set(1, 2);
  }
}

public class TestReferenceClass
{
  public TestInstantiated TestObjectReference;

  public TestReferenceClass()
  {    
    TestObjectReference = null;
  }
}