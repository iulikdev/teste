<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { callsHub } from '$lib/signalr';
	import { calls, statistics, connectionState, isInTeams } from '$lib/stores';
	import type { CallInfo, CallStatistics } from '$lib/types';
	import StatCard from '$lib/components/StatCard.svelte';
	import CallList from '$lib/components/CallList.svelte';
	import ConnectionStatus from '$lib/components/ConnectionStatus.svelte';

	let interval: ReturnType<typeof setInterval>;

	onMount(async () => {
		// Setup SignalR callbacks
		callsHub.onInitialState((state) => {
			calls.set(state.Calls || []);
			statistics.set(state.Statistics);
		});

		callsHub.onCallStarted((call: CallInfo) => {
			calls.update((c) => [...c, call]);
		});

		callsHub.onCallEnded((callId: string) => {
			calls.update((c) => c.filter((call) => call.callId !== callId));
		});

		callsHub.onCallUpdated((call: CallInfo) => {
			calls.update((c) => c.map((existing) => (existing.callId === call.callId ? call : existing)));
		});

		callsHub.onStatisticsUpdated((stats: CallStatistics) => {
			statistics.set(stats);
		});

		// Connect to SignalR
		await callsHub.start();

		// Update connection state periodically
		interval = setInterval(() => {
			connectionState.set(callsHub.state);
		}, 1000);
	});

	onDestroy(() => {
		if (interval) clearInterval(interval);
		callsHub.stop();
	});

	function formatDuration(duration: string | undefined): string {
		if (!duration) return '00:00';
		const parts = duration.split(':');
		if (parts.length >= 2) {
			return `${parts[1]}:${parts[2]?.split('.')[0] || '00'}`;
		}
		return duration;
	}
</script>

<div class="dashboard" class:in-teams={$isInTeams}>
	<header>
		<div class="header-left">
			<h1>Call Center Dashboard</h1>
			{#if $isInTeams}
				<span class="teams-badge">Teams</span>
			{/if}
		</div>
		<ConnectionStatus state={$connectionState} />
	</header>

	<main>
		<section class="stats-grid">
			<StatCard
				value={$statistics?.activeCalls ?? 0}
				label="Apeluri Active"
				color="var(--teams-green)"
			/>
			<StatCard
				value={$statistics?.callsInQueue ?? 0}
				label="In Asteptare"
				color="var(--teams-yellow)"
			/>
			<StatCard
				value={$statistics?.totalCallsToday ?? 0}
				label="Total Azi"
				color="var(--teams-purple)"
			/>
			<StatCard
				value={$statistics?.answeredCalls ?? 0}
				label="Raspunse"
				color="var(--teams-blue)"
			/>
			<StatCard value={$statistics?.missedCalls ?? 0} label="Pierdute" color="var(--teams-red)" />
			<StatCard
				value={formatDuration($statistics?.averageCallDuration)}
				label="Durata Medie"
				color="var(--text-secondary)"
			/>
			<StatCard
				value={$statistics?.availableAgents ?? 0}
				label="Agenti Disponibili"
				color="var(--teams-green)"
			/>
			<StatCard
				value={$statistics?.busyAgents ?? 0}
				label="Agenti Ocupati"
				color="var(--teams-yellow)"
			/>
		</section>

		<section class="calls-section">
			<CallList calls={$calls} />
		</section>
	</main>
</div>

<style>
	.dashboard {
		min-height: 100vh;
		padding: 20px;
	}

	.dashboard.in-teams {
		padding: 12px;
	}

	header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 24px;
	}

	.header-left {
		display: flex;
		align-items: center;
		gap: 12px;
	}

	h1 {
		font-size: 1.5rem;
		font-weight: 600;
	}

	.teams-badge {
		background: var(--teams-purple);
		color: white;
		padding: 4px 8px;
		border-radius: 4px;
		font-size: 0.75rem;
	}

	main {
		display: flex;
		flex-direction: column;
		gap: 24px;
	}

	.stats-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
		gap: 16px;
	}

	.calls-section {
		flex: 1;
	}

	@media (max-width: 768px) {
		.stats-grid {
			grid-template-columns: repeat(2, 1fr);
		}
	}
</style>
