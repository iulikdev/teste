<script lang="ts">
	import type { CallInfo } from '$lib/types';
	import { CallStatus } from '$lib/types';

	export let calls: CallInfo[] = [];

	function getStatusClass(status: CallStatus): string {
		switch (status) {
			case CallStatus.Connected:
				return 'status-connected';
			case CallStatus.Ringing:
				return 'status-ringing';
			case CallStatus.InQueue:
				return 'status-queue';
			case CallStatus.Ended:
				return 'status-ended';
			default:
				return '';
		}
	}

	function formatDuration(duration: string): string {
		// Duration comes as "HH:MM:SS" format
		if (!duration) return '00:00';
		const parts = duration.split(':');
		if (parts.length === 3) {
			const hours = parseInt(parts[0]);
			const mins = parts[1];
			const secs = parts[2].split('.')[0];
			if (hours > 0) {
				return `${hours}:${mins}:${secs}`;
			}
			return `${mins}:${secs}`;
		}
		return duration;
	}

	function formatTime(dateStr: string): string {
		if (!dateStr) return '';
		const date = new Date(dateStr);
		return date.toLocaleTimeString('ro-RO', { hour: '2-digit', minute: '2-digit' });
	}
</script>

<div class="call-list">
	<div class="header">
		<h3>Apeluri Active</h3>
		<span class="count">{calls.length}</span>
	</div>

	{#if calls.length === 0}
		<div class="empty">
			<p>Nu sunt apeluri active</p>
		</div>
	{:else}
		<div class="calls">
			{#each calls as call (call.callId)}
				<div class="call-item">
					<div class="call-info">
						<div class="caller-name">{call.displayName || 'Unknown'}</div>
						<div class="caller-details">
							{#if call.phoneNumber}
								<span>{call.phoneNumber}</span>
							{/if}
							{#if call.agentName}
								<span class="agent">Agent: {call.agentName}</span>
							{/if}
						</div>
					</div>
					<div class="call-meta">
						<span class="status-badge {getStatusClass(call.status)}">
							{call.status}
						</span>
						<span class="duration">{formatDuration(call.duration)}</span>
						<span class="start-time">{formatTime(call.startTime)}</span>
					</div>
				</div>
			{/each}
		</div>
	{/if}
</div>

<style>
	.call-list {
		background: var(--bg-secondary);
		border-radius: 8px;
		border: 1px solid var(--border-color);
		overflow: hidden;
	}

	.header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 16px;
		border-bottom: 1px solid var(--border-color);
	}

	.header h3 {
		font-size: 1rem;
		font-weight: 600;
	}

	.count {
		background: var(--teams-purple);
		color: white;
		padding: 2px 10px;
		border-radius: 12px;
		font-size: 0.875rem;
	}

	.empty {
		padding: 40px;
		text-align: center;
		color: var(--text-secondary);
	}

	.calls {
		max-height: 400px;
		overflow-y: auto;
	}

	.call-item {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 12px 16px;
		border-bottom: 1px solid var(--border-color);
		transition: background 0.2s;
	}

	.call-item:hover {
		background: var(--bg-tertiary);
	}

	.call-item:last-child {
		border-bottom: none;
	}

	.caller-name {
		font-weight: 500;
		margin-bottom: 4px;
	}

	.caller-details {
		font-size: 0.875rem;
		color: var(--text-secondary);
	}

	.caller-details span {
		margin-right: 12px;
	}

	.agent {
		color: var(--teams-blue);
	}

	.call-meta {
		display: flex;
		align-items: center;
		gap: 12px;
	}

	.duration {
		font-family: 'Consolas', monospace;
		font-size: 0.875rem;
		color: var(--text-secondary);
	}

	.start-time {
		font-size: 0.75rem;
		color: var(--text-secondary);
	}
</style>
