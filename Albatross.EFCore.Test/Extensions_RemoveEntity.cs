using Albatross.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Albatross.EFCore.Test {
	public class Extensions_RemoveEntity {
		record Item(Guid Id, string Name);

		List<Item> BuildList(int count) =>
			Enumerable.Range(0, count).Select(_ => new Item(Guid.NewGuid(), "item")).ToList();

		[Fact]
		public void EmptyKeys_ReturnsEmpty() {
			var list = BuildList(3);
			var result = list.RemoveEntity(Array.Empty<Guid>(), x => x.Id);
			Assert.Empty(result);
			Assert.Equal(3, list.Count);
		}

		[Fact]
		public void EmptyList_ReturnsEmpty() {
			var list = new List<Item>();
			var result = list.RemoveEntity([Guid.NewGuid()], x => x.Id);
			Assert.Empty(result);
		}

		[Fact]
		public void SingleKey_RemovesCorrectItem() {
			var list = BuildList(3);
			var target = list[1];

			var result = list.RemoveEntity([target.Id], x => x.Id);

			Assert.Equal([target], result);
			Assert.Equal(2, list.Count);
			Assert.DoesNotContain(target, list);
		}

		[Fact]
		public void MultipleKeys_RemovesAllMatchingItems() {
			var list = BuildList(4);
			var targets = new[] { list[0], list[2] };

			var result = list.RemoveEntity(targets.Select(x => x.Id), x => x.Id);

			Assert.Equal(2, result.Count);
			Assert.All(targets, t => Assert.Contains(t, result));
			Assert.Equal(2, list.Count);
			Assert.All(targets, t => Assert.DoesNotContain(t, list));
		}

		[Fact]
		public void KeyNotFound_ThrowsNotFoundException() {
			var list = BuildList(3);
			var missingId = Guid.NewGuid();
			Assert.Throws<NotFoundException<Item>>(() => list.RemoveEntity([missingId], x => x.Id));
		}

		[Fact]
		public void SomeKeysNotFound_ThrowsBeforeAnyRemoval() {
			var list = BuildList(3);
			var validId = list[1].Id;
			var missingId = Guid.NewGuid();

			Assert.Throws<NotFoundException<Item>>(() =>
				list.RemoveEntity([validId, missingId], x => x.Id));

			Assert.Equal(3, list.Count);
		}

		[Fact]
		public void OneTimeEnumerable_WorksCorrectly() {
			var list = BuildList(3);
			var target = list[2];
			IEnumerable<Guid> keys = new[] { target.Id }.Where(_ => true); // one-time enumerable

			var result = list.RemoveEntity(keys, x => x.Id);

			Assert.Single(result);
			Assert.Equal(target, result[0]);
		}
	}
}
