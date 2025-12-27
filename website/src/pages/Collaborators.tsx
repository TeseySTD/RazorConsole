import { collaborators } from "@/data/collaborators"
import Collaborator from "@/components/collaborators/Collaborator"
import ContributeMessage from "@/components/collaborators/ContributeMessage"

export default function Collaborators() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-950 dark:to-slate-900">
      <div className="container mx-auto px-4 py-16">
        <div className="text-center mb-12">
          <h1 className="text-4xl font-bold tracking-tight text-slate-900 dark:text-slate-50 mb-4">
            Collaborators
          </h1>
          <p className="text-lg text-slate-600 dark:text-slate-300 max-w-2xl mx-auto">
            Meet the people who make RazorConsole possible
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 max-w-5xl mx-auto">
          {collaborators.map((collaborator) => (
            <Collaborator collaborator={collaborator} />
          ))}
        </div>

        <ContributeMessage />
      </div>
    </div>
  )
}
