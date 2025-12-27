import { cn } from "@/lib/utils";
import type { TopicItem } from "@/types/docs/topicItem";
import { ChevronDown, ChevronRight } from "lucide-react";

interface Props {
    topics: TopicItem[],
    releaseNotes: TopicItem[],
    activeTopic: TopicItem | null,
    expandedTopics: Set<string>,
    releaseNotesOpen: boolean,
    handleTopicClick: (topicId: string) => void,
    handleSubHeadingClick: (e: React.MouseEvent<HTMLButtonElement>, topicId: string, headingId: string) => void,
    toggleTopicExpand: (e: React.MouseEvent<HTMLButtonElement>, topicId: string) => void,
    setReleaseNotesOpen: (open: boolean) => void
}

const Sidebar: React.FC<Props> = (
    {
        topics,
        releaseNotes,
        activeTopic,
        expandedTopics,
        releaseNotesOpen,
        handleTopicClick,
        handleSubHeadingClick,
        toggleTopicExpand,
        setReleaseNotesOpen
    }
) => (
    <div className="space-y-6">
        <div>
            <h2 className="mb-4 text-xs font-semibold uppercase tracking-[0.2em] text-slate-500 dark:text-slate-400">Topics</h2>
            <nav className="space-y-1">
                {topics.map((topic) => {
                    const isActiveTopic = topic.id === activeTopic?.id;
                    const isExpanded = expandedTopics.has(topic.id);
                    return (
                        <div key={topic.id} className="space-y-1">
                            <div className="relative">
                                <button type="button" onClick={() => handleTopicClick(topic.id)} className={cn("w-full rounded-md px-3 py-2 text-left text-sm font-medium transition focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 focus-visible:ring-offset-slate-50 dark:focus-visible:ring-offset-slate-900 pr-8", isActiveTopic ? "bg-blue-500/10 text-blue-700 dark:bg-blue-500/20 dark:text-blue-100" : "text-slate-700 hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-800")}>
                                    {topic.title}
                                </button>
                                {topic.headings.length > 0 && (
                                    <button onClick={(e) => toggleTopicExpand(e, topic.id)} className="absolute right-2 top-1/2 -translate-y-1/2 p-1 text-slate-400 hover:text-slate-600 dark:text-slate-500 dark:hover:text-slate-300">
                                        {isExpanded ? <ChevronDown className="h-3.5 w-3.5" /> : <ChevronRight className="h-3.5 w-3.5" />}
                                    </button>
                                )}
                            </div>
                            {isExpanded && topic.headings.length > 0 && (
                                <div className="ml-4 border-l border-slate-200 dark:border-slate-800 pl-2 space-y-0.5 my-1">
                                    {topic.headings.map((heading, index) => {
                                        if (heading.level > 4) return null;
                                        return (
                                            <button key={`${topic.id}-${heading.id}-${index}`} type="button" onClick={(e) => handleSubHeadingClick(e, topic.id, heading.id)} className={cn("block w-full text-left text-xs py-1.5 px-2 rounded-md transition-colors", location.hash === `#${heading.id}` && isActiveTopic ? "text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900/20 font-medium" : "text-slate-500 hover:text-slate-800 hover:bg-slate-50 dark:text-slate-400 dark:hover:text-slate-200 dark:hover:bg-slate-800/50", heading.level === 3 && "pl-4", heading.level === 4 && "pl-6")}>
                                                {heading.title}
                                            </button>
                                        );
                                    })}
                                </div>
                            )}
                        </div>
                    );
                })}
            </nav>
        </div>
        <div className="pt-6 border-t border-slate-200 dark:border-slate-800">
            <button type="button" onClick={() => setReleaseNotesOpen(!releaseNotesOpen)} className="mb-3 flex w-full items-center justify-between text-xs font-semibold uppercase tracking-[0.2em] text-slate-500 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200 transition-colors">
                <span>Release Notes</span> {releaseNotesOpen ? <ChevronDown className="h-3 w-3" /> : <ChevronRight className="h-3 w-3" />}
            </button>
            {releaseNotesOpen && (
                <nav className="space-y-1">
                    {releaseNotes.map((note) => (
                        <button key={note.id} type="button" onClick={() => handleTopicClick(note.id)} className={cn("w-full rounded-md px-3 py-2 text-left text-sm font-medium transition focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 focus-visible:ring-offset-slate-50 dark:focus-visible:ring-offset-slate-900", note.id === activeTopic?.id ? "bg-blue-500/10 text-blue-700 dark:bg-blue-500/20 dark:text-blue-100" : "text-slate-700 hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-800")}>
                            {note.title}
                        </button>
                    ))}
                </nav>
            )}
        </div>
    </div>
);

export default Sidebar;