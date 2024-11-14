using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWatcherController : MonoBehaviour
{
    [SerializeField] float _detectPlayerRange = 10f;
    [SerializeField] float _attackRange = 2f;
    [SerializeField] float _idleDuration = 3f;
    [SerializeField] float _patrolSpeed = 2f;
    [SerializeField] float _chaseSpeed = 4f;
    [SerializeField] List<Transform> _patrolPoints;
    public float PatrolSpeed => _patrolSpeed;
    public float ChaseSpeed => _chaseSpeed;

    private IEnemyState _currentState;
    private Animator _anim;
    private Transform _player;
    private int _currentPatrolIndex = 0;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] float _idleTimeout = 3f;
    [SerializeField] Collider _hitAttackCollider;
    [SerializeField] LayerMask _targetLayer;
    [SerializeField] int[] _attackDamageAmount;
    public int[] AvailableAttacks => _attackDamageAmount;
    public float IdleTimeout => _idleTimeout;
    public bool IsAttacking { get; private set; }
    int _currentDamageAmount = 0;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _player = GameObject.FindWithTag("Player").transform;
    }

    private void Start()
    {
        SetState(new EnemyWatcherIdleState(this, _idleDuration));
    }

    private void Update()
    {
        _currentState.Execute();
    }

    public void SetState(IEnemyState newState)
    {
        _currentState = newState;
        _currentState.EnterState();
    }

    public void SetAnimator(string paramName, bool value)
    {
        _anim.SetBool(paramName, value);
    }

    public void TriggerAnimator(string name)
    {
        _anim.SetTrigger(name);
    }

    public bool IsPlayerClose() => Vector3.Distance(transform.position, _player.position) < _detectPlayerRange;
    public bool IsPlayerInAttackRange() => Vector3.Distance(transform.position, _player.position) <= _attackRange;
    public Transform GetNextPatrolPoint()
    {
        //Transform patrolPoint = _patrolPoints[_currentPatrolIndex];
        //_currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Count;
        //return patrolPoint;
        if (_patrolPoints.Count <= 1)
        {
            Debug.LogWarning("No hay suficientes puntos de patrulla disponibles.");
            return null;
        }
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, _patrolPoints.Count);
        } while (randomIndex == _currentPatrolIndex);

        _currentPatrolIndex = randomIndex;
        return _patrolPoints[_currentPatrolIndex];
    }

    public void MoveTowards(Vector3 target, float speed)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.LookAt(target);
    }

    public Transform GetPlayer() => _player;

    private void OnTriggerEnter(Collider other)
    {
        if (Utils.CheckLayerInMask(_targetLayer, other.gameObject.layer))
        {
            IDamagable recieveDamage = other.gameObject.GetComponent<IDamagable>();
            recieveDamage?.Damage(_currentDamageAmount);
            Debug.Log($"Le hago da�o de {_currentDamageAmount} a {other.name}");
        }
    }

    public void EnableAttack(int attackNumber)
    {
        _currentDamageAmount = _attackDamageAmount[attackNumber - 1];
        _hitAttackCollider.enabled = true;
        IsAttacking = true;
    }

    public void DisableAttack()
    {
        _hitAttackCollider.enabled = false;
        IsAttacking = false;
    }
}
