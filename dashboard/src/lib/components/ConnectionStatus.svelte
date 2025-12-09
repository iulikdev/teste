<script lang="ts">
	export let state: string = 'Disconnected';

	$: statusClass = state === 'Connected' ? 'connected' : 'disconnected';
</script>

<div class="connection-status {statusClass}">
	<span class="dot"></span>
	<span class="text">{state}</span>
</div>

<style>
	.connection-status {
		display: inline-flex;
		align-items: center;
		gap: 6px;
		padding: 4px 12px;
		border-radius: 16px;
		font-size: 0.75rem;
	}

	.dot {
		width: 8px;
		height: 8px;
		border-radius: 50%;
	}

	.connected {
		background: rgba(146, 195, 83, 0.2);
	}

	.connected .dot {
		background: var(--teams-green);
	}

	.connected .text {
		color: var(--teams-green);
	}

	.disconnected {
		background: rgba(196, 49, 75, 0.2);
	}

	.disconnected .dot {
		background: var(--teams-red);
	}

	.disconnected .text {
		color: var(--teams-red);
	}
</style>
