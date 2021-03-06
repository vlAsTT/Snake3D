using System;
using System.Collections.Generic;
using Snake.Gameplay;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Snake.Movement
{
    /// <summary>
    /// Specifies for directions that Snake can head towards
    /// Used only for initialization
    /// </summary>
    /// <seealso cref="MovementController"/>
    public enum PlayerDirection
    {
        Up = 0,
        Down,
        Left,
        Right,
        Count
    }

    /// <summary>
    /// Handles all movement-related functionality and also includes tail spawning functionality
    /// </summary>
    public class MovementController : MonoBehaviour
    {
        #region Variables

        #region Movement

        /// <summary>
        /// Base speed of the snake
        /// </summary>
        [Header("Movement Information")]
        [Tooltip("Initial speed of the snake")][SerializeField] private float baseSpeed = 10.0f;

        /// <summary>
        /// Current speed of the Snake
        /// </summary>
        private float _currentSpeed;

        /// <summary>
        /// Distance between snake body parts
        /// </summary>
        /// <seealso cref="InitSnakeParts"/>
        private static float BetweenBodyDistance => .6f;

        #endregion

        #region Tail Spawn Information

        /// <summary>
        /// Prefab that is being spawned when tail is enlarged
        /// </summary>
        /// <seealso cref="SpawnTail"/>
        [Header("Tail Spawn Information")]
        [SerializeField] private GameObject tailPrefab;

        #endregion

        #region References

        /// <summary>
        /// References to all child objects of a snake
        /// Including Head, Body & Tail from the initialization
        /// </summary>
        private List<Transform> _snakeParts;
        
        /// <summary>
        /// Reference to the snake's head transform
        /// </summary>
        private Transform _snakeHead;

        #endregion

        #endregion

        #region Methods
        
        #region Unity Standard

        /// <summary>
        /// Initialization of all variables, references & setting up direction
        /// </summary>
        private void Start()
        {
            // Subscribing to the Item Update event
            ItemDelegates.onItemDestroy += SpawnTail;
            
            _currentSpeed = baseSpeed;
            
            InitSnakeParts();
            InitSnakeDirection();
        }

        /// <summary>
        /// Handles movement update every frame
        /// </summary>
        private void Update()
        {
            Move();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDestroy()
        {
            // Unsubscribing from the Item Update event
            ItemDelegates.onItemDestroy -= SpawnTail;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes Direction of the snake, resets body to be heading in the right direction and sets all forward vectors
        /// </summary>
        /// <seealso cref="PlayerDirection"/>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void InitSnakeDirection()
        {
            // Obtains random starting direction
            var currentDirection = GetRandomDirection();

            // Depending on the randomly obtained direction - sets body positions & forward vectors accordingly
            switch (currentDirection)
            {
                case PlayerDirection.Up:
                    for (int i = 1; i < _snakeParts.Count; i++)
                    {
                        _snakeParts[i].position = _snakeParts[0].position - new Vector3(0f, 0f, BetweenBodyDistance * i);
                        _snakeParts[i].forward = Vector3.forward;
                    }

                    _snakeHead.forward = Vector3.forward;
                    break;
                case PlayerDirection.Down:
                    for (int i = 1; i < _snakeParts.Count; i++)
                    {
                        _snakeParts[i].position = _snakeParts[0].position + new Vector3(0f, 0f, BetweenBodyDistance * i);
                        _snakeParts[i].forward = Vector3.back;
                    }

                    _snakeHead.forward = Vector3.back;
                    break;
                case PlayerDirection.Left:
                    for (int i = 1; i < _snakeParts.Count; i++)
                    {
                        _snakeParts[i].position = _snakeParts[0].position + new Vector3(BetweenBodyDistance * i, 0f, 0f);
                        _snakeParts[i].forward = Vector3.left;
                    }

                    _snakeHead.forward = Vector3.left;
                    break;
                case PlayerDirection.Right:
                    for (int i = 1; i < _snakeParts.Count; i++)
                    {
                        _snakeParts[i].position = _snakeParts[0].position - new Vector3(BetweenBodyDistance * i, 0f, 0f);
                        _snakeParts[i].forward = Vector3.right;
                    }

                    _snakeHead.forward = Vector3.right;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <summary>
        /// Initializes all references to snake body parts
        /// </summary>
        private void InitSnakeParts()
        {
            _snakeParts = new List<Transform>();

            // Iterate through child objects and add references to their rigidbodies
            for (int i = 0; i < transform.childCount; i++)
            {
                _snakeParts.Add(transform.GetChild(i).GetComponent<Transform>());
            }

            // If there are less than 2 rigidbodies in child objects - something is wrong with prefab as there should be at least a Head & Tail/Body parts
            if (_snakeParts.Count < 2)
            {
                Debug.LogError(
                    $"Snake prefab is misconfigured. There are less than 2 body parts with Rigidbodies at {transform.name}.");
                return;
            }

            _snakeHead = _snakeParts[0];
        }

        #endregion

        #region Movement

        /// <summary>
        /// Returns a random direction
        /// </summary>
        /// <see cref="PlayerDirection"/>
        /// <returns>Random PlayerDirection</returns>
        private PlayerDirection GetRandomDirection()
        {
            return (PlayerDirection)Random.Range(0, (int) PlayerDirection.Count);
        }

        /// <summary>
        /// Handles the movement of all snake body parts
        /// </summary>
        private void Move()
        {
            // Snake Head Movement
            _snakeParts[0].Translate(_snakeParts[0].forward * (_currentSpeed * Time.smoothDeltaTime), Space.World);

            // Movement of all other body parts
            for (int i = 1; i < _snakeParts.Count; i++)
            {
                var currentBodyPart = _snakeParts[i];
                var previousBodyPart = _snakeParts[i - 1];
                
                var newPosition = previousBodyPart.position;
                var currentPosition = currentBodyPart.position;
                
                // Calculating distance between new and current position and time to get there
                var distance = Vector3.Distance(newPosition, currentPosition);
                var T = Mathf.Clamp(Time.deltaTime * distance / 1f * _currentSpeed, 0f, 0.8f);
                
                // Sets new Position & Rotation
                currentPosition = Vector3.Slerp(currentPosition, newPosition, T);
                currentBodyPart.position = currentPosition;
                currentBodyPart.rotation = Quaternion.Slerp(currentBodyPart.rotation, previousBodyPart.rotation, T);
            }
        }
        
        /// <summary>
        /// Handles the player's input update and rotates snake head accordingly to received input
        /// </summary>
        /// <param name="directions">Player's Input</param>
        public void UpdateMovementDirection(Vector2 directions)
        {
            // Debug.Log($"Called Movement Controller: ({directions.x}, {directions.y}) ; Current Player's Forward Vector: {snakeHead.forward}");

            // Check for the current forward vector and what inputs are available to the user
            // For e.g. if snake is heading forward/down - buttons W/S won't do anything, same situation for W/S when
            // snake is heading left/right directions
            if (_snakeHead.forward == Vector3.back || _snakeHead.forward == Vector3.forward)
            {
                switch (directions.x)
                {
                    case 1f:
                        _snakeHead.forward = Vector3.right;
                        break;
                    case -1f:
                        _snakeHead.forward = Vector3.left;
                        break;
                }
            }
            else if (_snakeHead.forward == Vector3.left || _snakeHead.forward == Vector3.right)
            {
                switch (directions.y)
                {
                    case 1f:
                        _snakeHead.forward = Vector3.forward;
                        break;
                    case -1f:
                        _snakeHead.forward = Vector3.back;
                        break;
                }
            }
        }

        #endregion

        #region Gameplay

        /// <summary>
        /// Spawns a new tail part
        /// </summary>
        /// <seealso cref="tailPrefab"/>
        /// <seealso cref="_snakeParts"/>
        private void SpawnTail()
        {
            if (!tailPrefab)
            {
                Debug.LogError($"Tail Prefab is missing at {name}");
                return;;
            }
            
            var tailPart = Instantiate(tailPrefab, _snakeParts[_snakeParts.Count - 1].position, Quaternion.identity);
            
            _snakeParts.Add(tailPart.transform);
        }

        #endregion

        #endregion
    }
}
