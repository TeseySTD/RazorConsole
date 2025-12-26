import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import ApiDocument from '@/components/ApiDocument'
import { apiItems, apiToc, type DocfxApiItem, type DocfxTocNode } from '@/data/api-docs'
import { Search, ChevronRight, X } from 'lucide-react'
import { cn } from '@/lib/utils'
import { Button } from "@/components/ui/button"
import MobileNavOpenButton from '@/components/ui/mobileNavOpenButton'

function findFirstUid(nodes: DocfxTocNode[]): string | undefined {
  for (const node of nodes) {
    if (node.uid) {
      return node.uid
    }
    if (node.items) {
      const nested = findFirstUid(node.items)
      if (nested) {
        return nested
      }
    }
  }
  return undefined
}

function TocTree({
  nodes,
  activeUid,
  onSelect
}: {
  nodes: DocfxTocNode[]
  activeUid?: string
  onSelect: (uid: string) => void
}) {
  return (
    <ul className="space-y-1">
      {nodes.map(node => {
        const key = node.uid ?? node.name
        const isActive = node.uid === activeUid
        const isClickable = Boolean(node.uid)

        return (
          <li key={key} className="space-y-1">
            {isClickable ? (
              <button
                type="button"
                onClick={() => node.uid && onSelect(node.uid)}
                className={cn(
                  'w-full rounded-md px-3 py-2 text-left text-sm font-medium transition focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 focus-visible:ring-offset-slate-50 dark:focus-visible:ring-offset-slate-900',
                  isActive
                    ? 'bg-blue-500/15 text-blue-700 dark:bg-blue-500/20 dark:text-blue-100'
                    : 'text-slate-700 hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-800'
                )}
              >
                {node.name}
              </button>
            ) : (
              <div className="px-3 py-2 text-sm font-semibold uppercase tracking-[0.2em] text-slate-500 dark:text-slate-400">
                {node.name}
              </div>
            )}
            {node.items && node.items.length > 0 && (
              <div className="ml-4 border-l border-slate-200 pl-2 dark:border-slate-800">
                <TocTree nodes={node.items} activeUid={activeUid} onSelect={onSelect} />
              </div>
            )}
          </li>
        )
      })}
    </ul>
  )
}

export default function ApiDocs() {
  const navigate = useNavigate();
  const params = useParams<{ uid: string }>();
  const [query, setQuery] = useState('');
  const [mobileSidebarOpen, setMobileSidebarOpen] = useState(false);
  const docfxToc = apiToc;
  const docfxItems = apiItems;
  const decodedUid = params.uid ? decodeURIComponent(params.uid) : undefined;
  const firstUid = useMemo(() => findFirstUid(docfxToc), [docfxToc]);

  useEffect(() => {
    if (!decodedUid && firstUid) navigate(`/api/${encodeURIComponent(firstUid)}`, { replace: true });
  }, [decodedUid, firstUid, navigate]);

  useEffect(() => { setMobileSidebarOpen(false); }, [decodedUid]);

  const activeItem = decodedUid ? docfxItems[decodedUid] : undefined;
  const searchTerm = query.trim().toLowerCase();
  const searchResults = useMemo(() => {
    if (!searchTerm) return [];
    return (Object.values(docfxItems) as DocfxApiItem[]).filter(item => {
      const name = item.name.toLowerCase();
      const fullName = (item.fullName ?? '').toLowerCase();
      return name.includes(searchTerm) || fullName.includes(searchTerm);
    }).slice(0, 25);
  }, [docfxItems, searchTerm]);

  const handleSelect = (uid: string) => { navigate(`/api/${encodeURIComponent(uid)}`); setQuery(''); };

  const SidebarContent = () => (
    <div className="space-y-3">
      <div className="relative">
        <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
        <input type="search" className="w-full rounded-lg border border-slate-200 bg-white py-2 pl-9 pr-3 text-sm text-slate-700 shadow-sm outline-none transition focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-200 dark:focus:border-blue-400 dark:focus:ring-blue-500/40" placeholder="Search types..." value={query} onChange={event => setQuery(event.target.value)} />
      </div>
      {searchTerm ? (
        <div className="space-y-1">
          {searchResults.length === 0 && <p className="px-3 py-2 text-sm text-slate-500 dark:text-slate-400">No matches found.</p>}
          {searchResults.map(result => (
            <button key={result.uid} type="button" onClick={() => handleSelect(result.uid)} className={cn('w-full rounded-md px-3 py-2 text-left text-sm transition focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 focus-visible:ring-offset-slate-50 dark:focus-visible:ring-offset-slate-900', result.uid === decodedUid ? 'bg-blue-500/15 text-blue-700 dark:bg-blue-500/20 dark:text-blue-100' : 'text-slate-700 hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-800')}>
              <span className="block font-medium">{result.name}</span>
              <span className="block text-xs text-slate-500 dark:text-slate-400">{result.fullName ?? result.uid}</span>
            </button>
          ))}
        </div>
      ) : <TocTree nodes={docfxToc} activeUid={decodedUid} onSelect={handleSelect} />}
    </div>
  );

  const scrollbarStyles = "[&::-webkit-scrollbar]:w-2 [&::-webkit-scrollbar-track]:bg-transparent [&::-webkit-scrollbar-thumb]:bg-slate-200 dark:[&::-webkit-scrollbar-thumb]:bg-slate-800 [&::-webkit-scrollbar-thumb]:rounded-full hover:[&::-webkit-scrollbar-thumb]:bg-slate-300 dark:hover:[&::-webkit-scrollbar-thumb]:bg-slate-700";

  return (
    <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-950 dark:to-slate-900">
      <div className="px-6 py-16 sm:px-10 lg:px-16">
        <div className="flex flex-col lg:block">
          <aside className={`hidden lg:block fixed left-0 top-0 bottom-0 w-72 z-40 overflow-y-auto border-r border-slate-200 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-950/50 backdrop-blur-xl px-6 py-6 ${scrollbarStyles}`}>
            <SidebarContent />
          </aside>

          <MobileNavOpenButton setMobileSidebarOpen={setMobileSidebarOpen} />

          <div className={cn("lg:hidden fixed inset-0 z-[60] transition-all duration-300 ease-in-out", mobileSidebarOpen ? "visible" : "invisible pointer-events-none")}>
            <div className={cn("absolute inset-0 bg-black/80 backdrop-blur-sm transition-opacity duration-300", mobileSidebarOpen ? "opacity-100" : "opacity-0")} onClick={() => setMobileSidebarOpen(false)} />
            <div className={cn(
              "absolute inset-y-0 left-0 w-3/4 max-w-xs bg-white dark:bg-slate-950 border-r border-slate-200 dark:border-slate-800 shadow-2xl p-6 overflow-y-auto transition-transform duration-300 ease-in-out",
              scrollbarStyles,
              mobileSidebarOpen ? "translate-x-0" : "-translate-x-full"
            )}>
              <div className="flex justify-end mb-4">
                <Button variant="ghost" size="icon" className="rounded-full" onClick={() => setMobileSidebarOpen(false)}><X className="h-5 w-5" /></Button>
              </div>
              <SidebarContent />
            </div>
          </div>
          <main className="flex-1 min-w-0"><ApiDocument item={activeItem} /></main>
        </div>
      </div>
    </div>
  )
}