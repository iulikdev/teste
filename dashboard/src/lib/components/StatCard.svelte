<script lang="ts">
	export let value: string | number;
	export let label: string;
	export let color: string = 'var(--teams-purple)';
</script>

<div class="card stat-card">
	<div class="stat-value" style="color: {color}">{value}</div>
	<div class="stat-label">{label}</div>
</div>

<style>
	.stat-card {
		min-width: 120px;
	}
</style>
