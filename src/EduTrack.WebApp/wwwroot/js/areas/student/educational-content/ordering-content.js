
(function () {
	function initOrdering() {
		var list = document.getElementById('orderingList');
		if (!list) return;

		var allowDnd = (list.getAttribute('data-allow-dnd') || 'true') === 'true';
		var dragSrcEl = null;

		function handleDragStart(e) {
			if (!allowDnd) return;
			dragSrcEl = this;
			e.dataTransfer.effectAllowed = 'move';
			e.dataTransfer.setData('text/plain', this.dataset.idx || '');
			this.classList.add('dragging');
		}

		function handleDragOver(e) {
			if (!allowDnd) return false;
			if (e.preventDefault) e.preventDefault();
			e.dataTransfer.dropEffect = 'move';
			return false;
		}

		function handleDragEnter() { this.classList.add('over'); }
		function handleDragLeave() { this.classList.remove('over'); }

		function handleDrop(e) {
			if (!allowDnd) return false;
			if (e.stopPropagation) e.stopPropagation();
			if (dragSrcEl !== this) {
				var parent = this.parentNode;
				var srcNext = dragSrcEl.nextSibling === this ? dragSrcEl : dragSrcEl.nextSibling;
				parent.insertBefore(dragSrcEl, this);
				parent.insertBefore(this, srcNext);
			}
			return false;
		}

		function handleDragEnd() {
			Array.prototype.forEach.call(list.children, function (item) {
				item.classList.remove('over');
				item.classList.remove('dragging');
			});
		}

		Array.prototype.forEach.call(list.children, function (item) {
			item.setAttribute('draggable', allowDnd ? 'true' : 'false');
			item.addEventListener('dragstart', handleDragStart, false);
			item.addEventListener('dragenter', handleDragEnter, false);
			item.addEventListener('dragover', handleDragOver, false);
			item.addEventListener('dragleave', handleDragLeave, false);
			item.addEventListener('drop', handleDrop, false);
			item.addEventListener('dragend', handleDragEnd, false);
		});
	}

	if (document.readyState === 'loading') {
		document.addEventListener('DOMContentLoaded', initOrdering);
	} else {
		initOrdering();
	}
})();


