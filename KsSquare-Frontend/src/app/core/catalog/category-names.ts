export function isPendantCategory(name:string):boolean {return name.toLowerCase().split(/[\s_-]+/).some(word=>['pendant','pendants','pendate','pendates','pandate','pandates','pandant','pandants'].includes(word));}

export function matchesCollectionCategory(name: string, collection: string): boolean {
  if (collection === 'pendant') return isPendantCategory(name);
  const words = name.toLowerCase().trim().split(/[\s_-]+/);
  if (collection === 'watch') return words.some(word => ['watch', 'watches'].includes(word));
  if (collection === 'grill') return words.some(word => ['grill', 'grills', 'grillz'].includes(word));
  return name.toLowerCase().trim().startsWith(collection.toLowerCase());
}
