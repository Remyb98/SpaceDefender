﻿using SDD.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public abstract class Enemy : SimpleGameStateObserver,IScore {

	protected Rigidbody m_Rigidbody;
	protected Transform m_Transform;

	[Header("Movement")]
	[SerializeField] private float m_MaxTranslationSpeed;
	[SerializeField] private float m_MinTranslationSpeed;
	[SerializeField] private AnimationCurve m_TranslationSpeedProbaCurve;
	protected float m_TranslationSpeed;

	[Header("Shoot")]
	[SerializeField] private GameObject m_BulletPrefab;
	[SerializeField] private float m_ShootPeriod;
	[SerializeField] private float m_ProbaShoot;
	[SerializeField] private Transform m_BulletSpawnPoint;
	private float m_NextShootTime;

	public float TranslationSpeed { get { return m_TranslationSpeed; } }

	protected abstract Vector3 MoveVect { get; }

	[Header("Score")]
	[SerializeField] int m_Score;
	public int Score { get { return m_Score; } }

	protected bool m_Destroyed = false;

	protected override void Awake()
	{
		base.Awake();

		m_Rigidbody = GetComponent<Rigidbody>();
		m_Transform = GetComponent<Transform>();
		m_NextShootTime = Time.time;


		m_TranslationSpeed = Mathf.Lerp(m_MinTranslationSpeed, m_MaxTranslationSpeed, m_TranslationSpeedProbaCurve.Evaluate(Random.value));
	}

	public virtual void Update()
	{
		if (Camera.main.WorldToViewportPoint(m_Transform.position).x < -.1f)
		{
			EventManager.Instance.Raise(new EnemyHasBeenDestroyedEvent() { eEnemy = this, eDestroyedByPlayer = false });
			m_Destroyed = true;
			Destroy(gameObject);
		}

		//Fire
		float probaShoot = Random.Range(0f, 1f);
		if (m_NextShootTime < Time.time && probaShoot <= m_ProbaShoot)
		{
			ShootBullet();
			m_NextShootTime = Time.time + m_ShootPeriod;
		}
	}

	public virtual void FixedUpdate()
	{
		if (!GameManager.Instance.IsPlaying) return;
		m_Rigidbody.MovePosition(m_Rigidbody.position + MoveVect);

	}

	void ShootBullet()
	{
		GameObject bulletGO = Instantiate(m_BulletPrefab, m_BulletSpawnPoint.position, Quaternion.identity);
	}

	private void OnCollisionEnter(Collision collision)
	{
		Debug.Log( name+" Collision with " + collision.gameObject.name);
		if(collision.gameObject.CompareTag("PlayerBullet")
			|| collision.gameObject.CompareTag("Player"))
		{
			EventManager.Instance.Raise(new ScoreItemEvent() { eScore = this as IScore });
			EventManager.Instance.Raise(new EnemyHasBeenDestroyedEvent() { eEnemy = this,eDestroyedByPlayer = true });
			m_Destroyed = true;
			Destroy(gameObject);
		}
	}
}
