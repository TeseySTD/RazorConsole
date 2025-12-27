import ImageBanner from "@/components/showcase/ImageBanner"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/Card"
import { showcaseProjects } from "@/data/showcase"
import { Rocket } from "lucide-react"


export default function Showcase() {
  const getProjectUrl = (project: typeof showcaseProjects[0]) => {
    if (project.github) return `https://github.com/${project.github}`
    if (project.website) return project.website
    return undefined
  }

  return (
    <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-950 dark:to-slate-900">
      <div className="container mx-auto px-4 py-16">
        <div className="text-center mb-12">
          <h1 className="text-4xl font-bold tracking-tight text-slate-900 dark:text-slate-50 mb-4">
            Showcase
          </h1>
          <p className="text-lg text-slate-600 dark:text-slate-300 max-w-2xl mx-auto">
            Discover projects built with RazorConsole
          </p>
        </div>

        {showcaseProjects.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8 max-w-6xl mx-auto">
            {showcaseProjects.map((project) => {
              const projectUrl = getProjectUrl(project)
              return (
                <a
                  key={project.name}
                  href={projectUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="block transition-transform hover:scale-[1.02]"
                >
                  <Card className="flex flex-col h-full cursor-pointer hover:shadow-lg transition-shadow">
                    {project.imageUrls && project.imageUrls.length > 0 && (
                      <ImageBanner imageUrls={project.imageUrls} alt={project.name} />
                    )}
                    <CardHeader>
                      <CardTitle className="text-xl">{project.name}</CardTitle>
                    </CardHeader>
                    <CardContent className="flex-1 flex flex-col">
                      <CardDescription className="flex-1">
                        {project.description}
                      </CardDescription>
                    </CardContent>
                  </Card>
                </a>
              )
            })}
          </div>
        ) : (
          <div className="text-center py-12">
            <Rocket className="w-16 h-16 mx-auto text-slate-300 dark:text-slate-600 mb-4" />
            <p className="text-lg text-slate-600 dark:text-slate-400 mb-2">
              No projects showcased yet.
            </p>
            <p className="text-sm text-slate-500 dark:text-slate-500">
              Be the first to add your project!
            </p>
          </div>
        )}
      </div>
    </div>
  )
}
