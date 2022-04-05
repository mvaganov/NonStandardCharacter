using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Character {
	public class CharacterSwitcher : MonoBehaviour {
		public List<Root> Characters = new List<Root>();
		public void DataPopulator(List<object> data) {
			data.AddRange(Characters);
		}
	}
}