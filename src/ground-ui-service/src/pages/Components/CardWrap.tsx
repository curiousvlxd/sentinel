interface CardWrapProps {
  children: React.ReactNode;
  tall?: boolean;
}

const CardWrap = ({ children, tall }: CardWrapProps) => {
  return (
    <div className="flex w-full items-center justify-center">
      <div
        className={
          tall
            ? "w-full min-h-[92vh] rounded-2xl relative border border-slate-200 dark:border-slate-600 bg-white shadow-lg dark:bg-[#263244] dark:shadow-[0_10px_30px_rgba(0,0,0,0.25)]"
            : "w-full h-[80vh] rounded-2xl relative border border-slate-200 dark:border-slate-600 bg-white shadow-lg dark:bg-[#263244] dark:shadow-[0_10px_30px_rgba(0,0,0,0.25)]"
        }
      >
        <div className="h-full min-h-0 overflow-y-auto px-6 py-4 text-slate-900 dark:text-indigo-100">
          {children}
        </div>
      </div>
    </div>
  );
};

export default CardWrap;
