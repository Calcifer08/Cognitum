using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollBarVertical : MonoBehaviour
{
  void Start()
  {
    Scrollbar scrollbar = GetComponent<Scrollbar>();
    if (scrollbar != null)
    {
      scrollbar.value = 1;
    }
  }
}
