using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CONSTANTS : MonoBehaviour
{
    public const int GROUND_LAYER = 8;
    public const int PUSHABLE_LAYER = 9;
    public const int NPC_LAYER = 10;
    public const int CEILING_LAYER = (1 << 11) | (1 << 14);
    public const int INTERACT_LAYER = 12;
    public const int SLOW_DOWN_LAYER = 13;
    public const int ENEMY_LAYER = 15;
    public const int HURTBOX_LAYER = 16;

    public const int GROUND_MASK = (1 << GROUND_LAYER) |
                                  (1 << PUSHABLE_LAYER) |
                                  (1 << NPC_LAYER) |
                                  (1 << SLOW_DOWN_LAYER);

    public const float DIALOGUE_INPUT_DELAY = .25f;

    public const float NPC_ROTATE_SPEED = .15f;
}
